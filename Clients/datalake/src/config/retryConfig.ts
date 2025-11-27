/**
 * Конфигурация для retry механизма при обработке ошибок API
 */

export const RETRY_CONFIG = {
	MAX_RETRIES: 3,
	INITIAL_DELAY: 1000, // 1 секунда
	MAX_DELAY: 30000, // 30 секунд
	BACKOFF_MULTIPLIER: 2,
	RETRYABLE_STATUS_CODES: [502, 503, 504, 429], // Коды статусов, для которых нужно делать retry
	NON_RETRYABLE_STATUS_CODES: [400, 401, 403, 404], // Коды статусов, для которых НЕ нужно делать retry
} as const
