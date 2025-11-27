import { Api } from '@/generated/Api'
import {
	SourceActivityInfo,
	SourceUpdateRequest,
	SourceWithSettingsAndTagsInfo,
	SourceWithSettingsInfo,
} from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { computed, makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

export class SourcesStore extends BaseCacheStore {
	// Observable данные
	private sourcesCache = observable.box<SourceWithSettingsInfo[]>([])
	private sourcesByIdCache = observable.map<number, SourceWithSettingsAndTagsInfo>()
	private activityCache = observable.map<number, SourceActivityInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			activityState: computed,
		})
	}

	//#region Методы получения

	/**
	 * Получает список всех источников
	 * @returns Список источников из кэша (может быть пустым, пока данные загружаются)
	 */
	public get sources(): SourceWithSettingsInfo[] {
		return this.sourcesCache.get()
	}

	/**
	 * Получает конкретный источник по ID
	 * @param id Идентификатор источника
	 * @returns Информация об источнике или undefined
	 */
	public getSourceById(id: number): SourceWithSettingsAndTagsInfo | undefined {
		return this.sourcesByIdCache.get(id)
	}

	/**
	 * Реактивное состояние активности источников
	 * Компоненты могут использовать этот геттер для подписки на изменения
	 */
	public get activityState(): Map<number, SourceActivityInfo> {
		return new Map(this.activityCache.entries())
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка источников
	 */
	public refreshSources() {
		this.tryLoadSources()
	}

	/**
	 * Сигнал обновления конкретного источника
	 */
	public refreshSourceById(id: number) {
		this.tryLoadSourceById(id)
	}

	/**
	 * Сигнал обновления активности указанных источников
	 */
	public refreshActivity(sourceIds: number[]) {
		this.tryLoadActivity(sourceIds)
	}

	/**
	 * Создание нового источника
	 */
	public async createSource(): Promise<number | undefined> {
		const cacheKey = 'creating-source'

		if (this.isLoading(cacheKey)) return undefined
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesCreate()

			runInAction(() => {
				logger.info('Source created successfully', {
					component: 'SourcesStore',
					method: 'createSource',
					sourceId: response.data,
				})

				this.tryLoadSources()
			})

			return response.data
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create source'), {
				component: 'SourcesStore',
				method: 'createSource',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Обновление источника
	 */
	public async updateSource(id: number, data: SourceUpdateRequest): Promise<void> {
		const cacheKey = `updating-source-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventorySourcesUpdate(id, data)

			runInAction(() => {
				logger.info('Source updated successfully', {
					component: 'SourcesStore',
					method: 'updateSource',
					sourceId: id,
				})

				this.refreshSources()
				this.refreshSourceById(id)
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update source'), {
				component: 'SourcesStore',
				method: 'updateSource',
				sourceId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Удаление источника
	 */
	public async deleteSource(id: number): Promise<void> {
		const cacheKey = `deleting-source-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventorySourcesDelete(id)

			runInAction(() => {
				logger.info('Source deleted successfully', {
					component: 'SourcesStore',
					method: 'deleteSource',
					sourceId: id,
				})

				this.refreshSources()
				this.sourcesByIdCache.delete(id) // удаляем кэшированные данные
				this.activityCache.delete(id) // удаляем кэш активности
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete source'), {
				component: 'SourcesStore',
				method: 'deleteSource',
				sourceId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion

	//#region Приватные методы загрузки

	private async tryLoadSources(): Promise<void> {
		const cacheKey = 'sources'

		if (this.isLoading(cacheKey)) return
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

	private async tryLoadSourceById(id: number): Promise<void> {
		const cacheKey = `source_${id}`

		if (this.isLoading(cacheKey)) return
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
			logger.error(error instanceof Error ? error : new Error('Failed to load source by id'), {
				component: 'SourcesStore',
				method: 'tryLoadSourceById',
				sourceId: id,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	private async tryLoadActivity(sourceIds: number[]): Promise<void> {
		if (sourceIds.length === 0) return

		const cacheKey = 'activity'

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataSourcesGetActivity(sourceIds)
			runInAction(() => {
				response.data.forEach((activity) => {
					this.activityCache.set(activity.sourceId, activity)
				})
				this.setLastFetchTime(cacheKey)

				logger.debug('Activities refreshed successfully', {
					component: 'SourcesStore',
					method: 'refreshActivities',
					sourceIds: sourceIds,
					updatedCount: response.data.length,
				})
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load sources activity'), {
				component: 'SourcesStore',
				method: 'tryLoadActivity',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion
}
