/**
 * Сервис централизованного логирования
 *
 * Уровни логирования:
 * - debug: отладочная информация (только в dev)
 * - info: информационные сообщения (только в dev)
 * - warn: предупреждения (всегда)
 * - error: ошибки (всегда)
 *
 * В dev-режиме все логи выводятся в консоль.
 * В prod-режиме только warn и error логируются (можно расширить для отправки на сервер).
 */

type LogLevel = 'debug' | 'info' | 'warn' | 'error'

interface LogContext {
	component?: string
	action?: string
	method?: string
	[key: string]: unknown
}

/**
 * Проверяет, находимся ли мы в режиме разработки
 */
const isDev = (): boolean => {
	return (
		window.location.hostname === 'localhost' ||
		window.location.hostname === '127.0.0.1' ||
		!window.location.hostname
	)
}

/**
 * Форматирует контекст для вывода
 */
const formatContext = (context?: LogContext): string => {
	if (!context || Object.keys(context).length === 0) {
		return ''
	}

	const parts: string[] = []
	if (context.component) parts.push(`[${context.component}]`)
	if (context.action) parts.push(`action: ${context.action}`)
	if (context.method) parts.push(`method: ${context.method}`)

	// Добавляем остальные поля контекста
	Object.entries(context).forEach(([key, value]) => {
		if (key !== 'component' && key !== 'action' && key !== 'method') {
			parts.push(`${key}: ${String(value)}`)
		}
	})

	return parts.length > 0 ? ` ${parts.join(', ')}` : ''
}

/**
 * Отправляет ошибку на сервер для мониторинга (опционально)
 * TODO: Реализовать отправку на сервер при необходимости
 */
const sendToServer = (level: LogLevel, message: string, error?: Error, context?: LogContext): void => {
	// В будущем здесь можно добавить отправку на сервер
	// Например, через API endpoint для логирования
	if (level === 'error' || level === 'warn') {
		// Пока не реализовано - можно добавить позже
		// await api.logs.create({ level, message, error: error?.stack, context })
	}
}

class Logger {
	/**
	 * Логирует отладочную информацию (только в dev)
	 */
	debug(...args: unknown[]): void {
		if (isDev()) {
			console.debug('[DEBUG]', ...args)
		}
	}

	/**
	 * Логирует информационное сообщение (только в dev)
	 */
	info(...args: unknown[]): void {
		if (isDev()) {
			console.info('[INFO]', ...args)
		}
	}

	/**
	 * Логирует предупреждение
	 */
	warn(...args: unknown[]): void {
		const message = args.map(arg => String(arg)).join(' ')
		console.warn('[WARN]', ...args)
		sendToServer('warn', message)
	}

	/**
	 * Логирует ошибку
	 * @param error Ошибка или сообщение об ошибке
	 * @param context Дополнительный контекст (компонент, действие и т.д.)
	 */
	error(error: Error | string, context?: LogContext): void {
		const errorMessage = error instanceof Error ? error.message : error
		const errorStack = error instanceof Error ? error.stack : undefined
		const contextStr = formatContext(context)

		console.error(`[ERROR]${contextStr}`, error)

		// В dev-режиме показываем stack trace
		if (isDev() && errorStack) {
			console.error('Stack trace:', errorStack)
		}

		sendToServer('error', errorMessage, error instanceof Error ? error : undefined, context)
	}
}

// Экспортируем singleton экземпляр
export const logger = new Logger()

// Экспортируем типы для использования в других местах
export type { LogContext, LogLevel }
