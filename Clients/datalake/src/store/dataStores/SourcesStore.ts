import { Api } from '@/generated/Api'
import { SourceActivityInfo, SourceWithSettingsAndTagsInfo, SourceWithSettingsInfo } from '@/generated/data-contracts'
import { CACHE_TTL } from '@/config/cacheConfig'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления источниками с кэшированием
 */
export class SourcesStore extends BaseCacheStore {
	// Кэш списка источников
	private _sourcesCache = observable.box<SourceWithSettingsInfo[] | null>(null)

	// Кэш отдельных источников по ID
	private _sourcesByIdCache = observable.map<number, SourceWithSettingsAndTagsInfo>()

	// Кэш состояний активности источников (ключ: sourceId)
	private _activityCache = observable.map<number, SourceActivityInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = CACHE_TTL.SOURCES.LIST
	private readonly TTL_ITEM = CACHE_TTL.SOURCES.ITEM
	private readonly TTL_ACTIVITY = CACHE_TTL.SOURCES.ACTIVITY

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getSources: true,
			getSourceById: true,
			getActivity: true,
			isLoadingSources: true,
			isLoadingSource: true,
			invalidateSource: true,
			refreshSources: true,
			refreshActivity: true,
		})
	}

	/**
	 * Получает список всех источников
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список источников из кэша (может быть пустым, пока данные загружаются)
	 */
	getSources(): SourceWithSettingsInfo[] {
		const cacheKey = 'sources_list'
		const cached = this._sourcesCache.get()

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadSources('get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'SourcesStore',
					method: 'getSources',
					action: 'loadSources',
				})
			})
		}

		// Возвращаем текущий кэш (может быть пустым)
		return cached ?? []
	}

	/**
	 * Получает конкретный источник по ID
	 * @param id Идентификатор источника
	 * @returns Информация об источнике или undefined
	 */
	getSourceById(id: number): SourceWithSettingsAndTagsInfo | undefined {
		const cacheKey = `source_${id}`
		const cached = this._sourcesByIdCache.get(id)

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadSourceById(id, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'SourcesStore',
					method: 'getSourceById',
					action: 'loadSourceById',
					sourceId: id,
				})
			})
		}

		// Возвращаем текущий кэш
		return cached
	}

	/**
	 * Получает состояния активности для указанных источников
	 * @param sourceIds Массив идентификаторов источников
	 * @returns Объект с состояниями активности (ключ: sourceId)
	 */
	getActivity(sourceIds: number[]): Record<number, SourceActivityInfo> {
		if (sourceIds.length === 0) return {}

		const result: Record<number, SourceActivityInfo> = {}
		const needRefresh: number[] = []

		// Проверяем кэш для каждого источника
		for (const sourceId of sourceIds) {
			const cached = this._activityCache.get(sourceId)
			if (cached) {
				result[sourceId] = cached
			}
			needRefresh.push(sourceId)
		}

		// Загружаем данные, если запрос не идет
		if (needRefresh.length > 0 && !this.isLoading('activity')) {
			this.loadActivity(needRefresh, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'SourcesStore',
					method: 'getActivity',
					action: 'loadActivity',
					sourceIds: needRefresh,
				})
			})
		}

		return result
	}

	/**
	 * Проверяет, идет ли загрузка источников
	 * @returns true, если идет загрузка
	 */
	isLoadingSources(): boolean {
		return this.isLoading('sources_list')
	}

	/**
	 * Проверяет, есть ли данные в кэше для списка источников
	 * @returns true, если данные были загружены хотя бы раз
	 */
	hasSourcesCache(): boolean {
		return this.hasCache('sources_list')
	}

	/**
	 * Проверяет, идет ли загрузка конкретного источника
	 * @param id Идентификатор источника
	 * @returns true, если идет загрузка
	 */
	isLoadingSource(id: number): boolean {
		return this.isLoading(`source_${id}`)
	}

	/**
	 * Инвалидирует кэш для конкретного источника
	 * @param id Идентификатор источника
	 */
	invalidateSource(id: number): void {
		runInAction(() => {
			this._sourcesByIdCache.delete(id)
			this._activityCache.delete(id)
			// Инвалидируем список, так как он может содержать этот источник
			this.invalidateCache('sources_list')
		})
	}

	/**
	 * Принудительно обновляет список источников
	 */
	async refreshSources(): Promise<void> {
		// Просто вызываем loadSources - он сам проверит, идет ли загрузка
		await this.loadSources('manual-refresh')
	}

	/**
	 * Принудительно обновляет состояния активности
	 * @param sourceIds Массив идентификаторов источников
	 */
	async refreshActivity(sourceIds: number[]): Promise<void> {
		for (const sourceId of sourceIds) {
			this.invalidateCache(`activity_${sourceId}`)
		}
		await this.loadActivity(sourceIds, 'manual-refresh')
	}

	/**
	 * Загружает список источников с сервера
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadSources(reason: string = 'unknown'): Promise<void> {
		const cacheKey = 'sources_list'

		if (this.isLoading(cacheKey)) {
			logger.debug(`[SourcesStore] Skipping loadSources - already loading`, {
				component: 'SourcesStore',
				method: 'loadSources',
				cacheKey,
				reason,
			})
			return
		}

		logger.info(`[SourcesStore] API Request: inventorySourcesGetAll`, {
			component: 'SourcesStore',
			method: 'loadSources',
			cacheKey,
			reason,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesGetAll({
				withCustom: true,
			})

			if (response.status === 200) {
				runInAction(() => {
					this._sourcesCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load sources'), {
				component: 'SourcesStore',
				method: 'loadSources',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретный источник по ID
	 * @param id Идентификатор источника
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadSourceById(id: number, reason: string = 'unknown'): Promise<void> {
		const cacheKey = `source_${id}`

		if (this.isLoading(cacheKey)) {
			logger.debug(`[SourcesStore] Skipping loadSourceById - already loading`, {
				component: 'SourcesStore',
				method: 'loadSourceById',
				cacheKey,
				reason,
				sourceId: id,
			})
			return
		}

		logger.info(`[SourcesStore] API Request: inventorySourcesGet`, {
			component: 'SourcesStore',
			method: 'loadSourceById',
			cacheKey,
			reason,
			sourceId: id,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesGet(id)

			if (response.status === 200) {
				runInAction(() => {
					this._sourcesByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load source ${id}`), {
				component: 'SourcesStore',
				method: 'loadSourceById',
				sourceId: id,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает состояния активности источников
	 * @param sourceIds Массив идентификаторов источников
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadActivity(sourceIds: number[], reason: string = 'unknown'): Promise<void> {
		if (sourceIds.length === 0) return

		const cacheKey = 'activity'

		if (this.isLoading(cacheKey)) {
			logger.debug(`[SourcesStore] Skipping loadActivity - already loading`, {
				component: 'SourcesStore',
				method: 'loadActivity',
				cacheKey,
				reason,
				sourceIds,
			})
			return
		}

		logger.info(`[SourcesStore] API Request: dataSourcesGetActivity`, {
			component: 'SourcesStore',
			method: 'loadActivity',
			cacheKey,
			reason,
			sourceIds,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataSourcesGetActivity(sourceIds)

			if (response.status === 200) {
				runInAction(() => {
					for (const activity of response.data) {
						this._activityCache.set(activity.sourceId, activity)
						this.setLastFetchTime(`activity_${activity.sourceId}`)
					}
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load sources activity'), {
				component: 'SourcesStore',
				method: 'loadActivity',
				sourceIds: sourceIds,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
