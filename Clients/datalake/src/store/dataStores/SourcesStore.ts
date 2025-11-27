import { Api } from '@/generated/Api'
import { SourceActivityInfo, SourceWithSettingsAndTagsInfo, SourceWithSettingsInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления источниками с кэшированием последнего успешного ответа
 */
export class SourcesStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getSources: true,
			getSourceById: true,
			getActivity: true,
		})
	}

	/**
	 * Получает список всех источников
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список источников из кэша (может быть пустым, пока данные загружаются)
	 */
	public getSources(): SourceWithSettingsInfo[] {
		const reactive = this.sourcesCache.get()
		this.tryLoadSources()
		return reactive ?? []
	}

	public refreshSources() {
		this.tryLoadSources()
	}

	private sourcesCacheKey = 'sources'
	private sourcesCache = observable.box<SourceWithSettingsInfo[]>([])

	private async tryLoadSources(): Promise<void> {
		const cacheKey = this.sourcesCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'SourcesStore',
				method: 'tryLoadSources',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesGetAll({
				withCustom: true,
			})

			if (response.status === 200) {
				runInAction(() => {
					this.sourcesCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load sources'), {
				component: 'SourcesStore',
				method: 'tryLoadSources',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает конкретный источник по ID
	 * @param id Идентификатор источника
	 * @returns Информация об источнике или undefined
	 */
	getSourceById(id: number): SourceWithSettingsAndTagsInfo | undefined {
		const reactive = this.sourcesByIdCache.get(id)
		this.tryLoadSourceById(id)
		return reactive
	}

	private sourcesByIdCacheKey = (id: number) => `source_${id}`
	private sourcesByIdCache = observable.map<number, SourceWithSettingsAndTagsInfo>()

	private async tryLoadSourceById(id: number): Promise<void> {
		const cacheKey = this.sourcesByIdCacheKey(id)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'SourcesStore',
				method: 'tryLoadSourceById',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesGet(id)

			if (response.status === 200) {
				runInAction(() => {
					this.sourcesByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load source ${id}`), {
				component: 'SourcesStore',
				method: 'tryLoadSourceById',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает состояния активности для указанных источников
	 * @param sourceIds Массив идентификаторов источников
	 * @returns Объект с состояниями активности (ключ: sourceId)
	 */
	getActivity(sourceIds: number[]): Record<number, SourceActivityInfo> {
		if (sourceIds.length === 0) return {}

		const result: Record<number, SourceActivityInfo> = {}
		for (const sourceId of sourceIds) {
			const cached = this.activityCache.get(sourceId)
			if (cached) {
				result[sourceId] = cached
			}
		}

		this.tryLoadActivity(sourceIds)
		return result
	}

	public refreshActivity(sourceIds: number[]) {
		this.tryLoadActivity(sourceIds)
	}

	private activityCacheKey = 'activity'
	private activityCache = observable.map<number, SourceActivityInfo>()

	private async tryLoadActivity(sourceIds: number[]): Promise<void> {
		if (sourceIds.length === 0) return

		const cacheKey = this.activityCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'SourcesStore',
				method: 'tryLoadActivity',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataSourcesGetActivity(sourceIds)

			if (response.status === 200) {
				runInAction(() => {
					for (const activity of response.data) {
						this.activityCache.set(activity.sourceId, activity)
						this.setLastFetchTime(`activity_${activity.sourceId}`)
					}
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load sources activity'), {
				component: 'SourcesStore',
				method: 'tryLoadActivity',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретного источника
	 * @param id Идентификатор источника
	 */
	invalidateSource(id: number): void {
		runInAction(() => {
			this.sourcesByIdCache.delete(id)
			this.activityCache.delete(id)
		})
	}
}
