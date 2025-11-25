import { Api } from '@/generated/Api'
import { SourceActivityInfo, SourceWithSettingsInfo } from '@/generated/data-contracts'
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
	private _sourcesByIdCache = observable.map<number, SourceWithSettingsInfo>()

	// Кэш состояний активности источников (ключ: sourceId)
	private _activityCache = observable.map<number, SourceActivityInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = 5 * 60 * 1000 // 5 минут для списка
	private readonly TTL_ITEM = 10 * 60 * 1000 // 10 минут для отдельного источника
	private readonly TTL_ACTIVITY = 30 * 1000 // 30 секунд для состояний активности

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
	 * @returns Список источников
	 */
	getSources(): SourceWithSettingsInfo[] {
		const cacheKey = 'sources_list'
		const cached = this._sourcesCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadSources().catch((error) => {
					logger.error(error instanceof Error ? error : new Error(String(error)), {
						component: 'SourcesStore',
						method: 'getSources',
						action: 'loadSources',
					})
				})
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadSources().catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'SourcesStore',
					method: 'getSources',
					action: 'loadSources',
				})
			})
		}

		return cached ?? []
	}

	/**
	 * Получает конкретный источник по ID
	 * @param id Идентификатор источника
	 * @returns Информация об источнике или undefined
	 */
	getSourceById(id: number): SourceWithSettingsInfo | undefined {
		const cacheKey = `source_${id}`
		const cached = this._sourcesByIdCache.get(id)

		if (cached && this.isCacheValid(cacheKey, this.TTL_ITEM)) {
			if (this.shouldRefresh(cacheKey, this.TTL_ITEM)) {
				this.loadSourceById(id).catch((error) => {
					logger.error(error instanceof Error ? error : new Error(String(error)), {
						component: 'SourcesStore',
						method: 'getSourceById',
						action: 'loadSourceById',
						sourceId: id,
					})
				})
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadSourceById(id).catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'SourcesStore',
					method: 'getSourceById',
					action: 'loadSourceById',
					sourceId: id,
				})
			})
		}

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
			const cacheKey = `activity_${sourceId}`
			const cached = this._activityCache.get(sourceId)

			if (cached && this.isCacheValid(cacheKey, this.TTL_ACTIVITY)) {
				result[sourceId] = cached
				if (this.shouldRefresh(cacheKey, this.TTL_ACTIVITY)) {
					needRefresh.push(sourceId)
				}
			} else {
				needRefresh.push(sourceId)
			}
		}

		// Обновляем в фоне, если нужно
		if (needRefresh.length > 0 && !this.isLoading('activity')) {
			this.loadActivity(needRefresh).catch((error) => {
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
		this.invalidateCache('sources_list')
		await this.loadSources()
	}

	/**
	 * Принудительно обновляет состояния активности
	 * @param sourceIds Массив идентификаторов источников
	 */
	async refreshActivity(sourceIds: number[]): Promise<void> {
		for (const sourceId of sourceIds) {
			this.invalidateCache(`activity_${sourceId}`)
		}
		await this.loadActivity(sourceIds)
	}

	/**
	 * Загружает список источников с сервера
	 */
	private async loadSources(): Promise<void> {
		const cacheKey = 'sources_list'

		if (this.isLoading(cacheKey)) return

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
	 */
	private async loadSourceById(id: number): Promise<void> {
		const cacheKey = `source_${id}`

		if (this.isLoading(cacheKey)) return

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
	 */
	private async loadActivity(sourceIds: number[]): Promise<void> {
		if (sourceIds.length === 0) return

		const cacheKey = 'activity'

		if (this.isLoading(cacheKey)) return

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
