import { RETRY_CONFIG } from '@/config/retryConfig'
import { logger } from '@/services/logger'

/**
 * Парсит заголовок Retry-After и возвращает задержку в миллисекундах
 * @param header Значение заголовка Retry-After (может быть число секунд или HTTP дата)
 * @returns Задержка в миллисекундах
 */
export function parseRetryAfter(header: string | null | undefined): number {
	if (!header) return RETRY_CONFIG.INITIAL_DELAY

	const value = header.trim()

	// Если это число (секунды)
	if (/^\d+$/.test(value)) {
		const seconds = parseInt(value, 10)
		return Math.min(seconds * 1000, RETRY_CONFIG.MAX_DELAY)
	}

	// Если это HTTP дата
	try {
		const date = new Date(value)
		if (!isNaN(date.getTime())) {
			const delay = date.getTime() - Date.now()
			return Math.max(0, Math.min(delay, RETRY_CONFIG.MAX_DELAY))
		}
	} catch {
		// Игнорируем ошибки парсинга даты
	}

	// По умолчанию возвращаем начальную задержку
	return RETRY_CONFIG.INITIAL_DELAY
}

/**
 * Проверяет, нужно ли делать retry для данной ошибки
 * @param statusCode HTTP статус код
 * @returns true, если нужно делать retry
 */
export function shouldRetry(statusCode: number | undefined): boolean {
	if (!statusCode) return false

	// Не retry для определенных кодов
	if (RETRY_CONFIG.NON_RETRYABLE_STATUS_CODES.includes(statusCode)) {
		return false
	}

	// Retry для определенных кодов
	return RETRY_CONFIG.RETRYABLE_STATUS_CODES.includes(statusCode)
}

/**
 * Вычисляет задержку перед следующей попыткой с экспоненциальным backoff
 * @param attempt Номер попытки (начиная с 0)
 * @param retryAfter Задержка из заголовка Retry-After (если есть)
 * @returns Задержка в миллисекундах
 */
export function calculateDelay(attempt: number, retryAfter?: number | null): number {
	// Если есть Retry-After, используем его
	if (retryAfter !== null && retryAfter !== undefined) {
		return Math.min(retryAfter, RETRY_CONFIG.MAX_DELAY)
	}

	// Иначе используем экспоненциальный backoff
	const delay = RETRY_CONFIG.INITIAL_DELAY * Math.pow(RETRY_CONFIG.BACKOFF_MULTIPLIER, attempt)
	return Math.min(delay, RETRY_CONFIG.MAX_DELAY)
}

/**
 * Выполняет функцию с retry механизмом
 * @param fn Функция для выполнения
 * @param options Опции retry
 * @returns Результат выполнения функции
 */
export async function retryWithBackoff<T>(
	fn: () => Promise<T>,
	options?: {
		maxRetries?: number
		onRetry?: (attempt: number, error: unknown) => void
		retryCondition?: (error: unknown) => boolean
	},
): Promise<T> {
	const maxRetries = options?.maxRetries ?? RETRY_CONFIG.MAX_RETRIES
	let lastError: unknown

	for (let attempt = 0; attempt <= maxRetries; attempt++) {
		try {
			return await fn()
		} catch (error) {
			lastError = error

			// Если это последняя попытка, выбрасываем ошибку
			if (attempt >= maxRetries) {
				break
			}

			// Проверяем условие retry
			if (options?.retryCondition && !options.retryCondition(error)) {
				break
			}

			// Вызываем callback перед retry
			if (options?.onRetry) {
				options.onRetry(attempt + 1, error)
			}

			// Вычисляем задержку
			const delay = calculateDelay(attempt)

			// Логируем попытку
			logger.warn(`Retry attempt ${attempt + 1}/${maxRetries} after ${delay}ms`, {
				component: 'retry',
				attempt: attempt + 1,
				delay,
			})

			// Ждем перед следующей попыткой
			await new Promise((resolve) => setTimeout(resolve, delay))
		}
	}

	// Если все попытки исчерпаны, выбрасываем последнюю ошибку
	throw lastError
}
