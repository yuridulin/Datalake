import { runInAction } from 'mobx'

/**
 * Базовый класс для stores с кэшированием данных
 * Предоставляет общую логику для работы с TTL и валидацией кэша
 */
export abstract class BaseCacheStore {
	// Время последнего запроса для каждого ключа кэша
	protected _lastFetchTime: Map<string, number> = new Map()

	// Состояния загрузки для каждого ключа
	protected _loadingStates: Map<string, boolean> = new Map()

	/**
	 * Проверяет, валиден ли кэш для указанного ключа и типа данных
	 * @param cacheKey Ключ кэша
	 * @param ttl Время жизни кэша в миллисекундах
	 * @returns true, если кэш валиден
	 */
	protected isCacheValid(cacheKey: string, ttl: number): boolean {
		const lastFetch = this._lastFetchTime.get(cacheKey)
		if (!lastFetch) return false
		return Date.now() - lastFetch < ttl
	}

	/**
	 * Проверяет, нужно ли обновить кэш (80% от TTL)
	 * @param cacheKey Ключ кэша
	 * @param ttl Время жизни кэша в миллисекундах
	 * @returns true, если нужно обновить
	 */
	protected shouldRefresh(cacheKey: string, ttl: number): boolean {
		const lastFetch = this._lastFetchTime.get(cacheKey)
		if (!lastFetch) return true
		const refreshThreshold = ttl * 0.8 // 80% от TTL
		return Date.now() - lastFetch >= refreshThreshold
	}

	/**
	 * Устанавливает время последнего запроса
	 * @param cacheKey Ключ кэша
	 */
	protected setLastFetchTime(cacheKey: string): void {
		runInAction(() => {
			this._lastFetchTime.set(cacheKey, Date.now())
		})
	}

	/**
	 * Устанавливает состояние загрузки
	 * @param cacheKey Ключ кэша
	 * @param loading Состояние загрузки
	 */
	protected setLoading(cacheKey: string, loading: boolean): void {
		runInAction(() => {
			this._loadingStates.set(cacheKey, loading)
		})
	}

	/**
	 * Проверяет, идет ли загрузка для указанного ключа
	 * @param cacheKey Ключ кэша
	 * @returns true, если идет загрузка
	 */
	protected isLoading(cacheKey: string): boolean {
		return this._loadingStates.get(cacheKey) ?? false
	}

	/**
	 * Инвалидирует кэш для указанного ключа
	 * @param cacheKey Ключ кэша
	 */
	protected invalidateCache(cacheKey: string): void {
		runInAction(() => {
			this._lastFetchTime.delete(cacheKey)
		})
	}

	/**
	 * Очищает весь кэш
	 */
	protected clearCache(): void {
		runInAction(() => {
			this._lastFetchTime.clear()
			this._loadingStates.clear()
		})
	}
}
