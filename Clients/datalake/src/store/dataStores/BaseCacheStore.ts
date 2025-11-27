import { runInAction } from 'mobx'

/**
 * Базовый класс для stores с кэшированием данных
 * Предоставляет общую логику для работы с TTL и валидацией кэша
 */
export abstract class BaseCacheStore {
	/** Время последнего запроса для каждого ключа кэша */
	protected lastFetchTime: Map<string, number> = new Map()

	/** Состояния загрузки для каждого ключа */
	protected loadingStates: Map<string, boolean> = new Map()

	/**
	 * Устанавливает время последнего запроса
	 * @param cacheKey Ключ кэша
	 */
	protected setLastFetchTime(cacheKey: string): void {
		runInAction(() => {
			this.lastFetchTime.set(cacheKey, Date.now())
		})
	}

	/**
	 * Устанавливает состояние загрузки
	 * @param cacheKey Ключ кэша
	 * @param loading Состояние загрузки
	 */
	protected setLoading(cacheKey: string, loading: boolean): void {
		runInAction(() => {
			this.loadingStates.set(cacheKey, loading)
		})
	}

	/**
	 * Проверяет, идет ли загрузка для указанного ключа
	 * @param cacheKey Ключ кэша
	 * @returns true, если идет загрузка
	 */
	protected isLoading(cacheKey: string): boolean {
		return this.loadingStates.get(cacheKey) ?? false
	}
}
