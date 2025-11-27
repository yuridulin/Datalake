import { Api } from '@/generated/Api'
import { DataValuesGetPayload, ValuesResponse } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления значениями тегов с кэшированием последнего успешного ответа
 */
export class ValuesStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getValues: true,
			getStatus: true,
		})
	}

	/**
	 * Получает значения тегов по запросу
	 * @param request Запрос на получение значений
	 * @returns Массив ответов с значениями
	 */
	public getValues(request: DataValuesGetPayload): ValuesResponse[] {
		const cacheKey = this.getCacheKey(request)
		const reactive = this.valuesCache.get(cacheKey)
		this.tryLoadValues(request)
		return reactive ?? []
	}

	public refreshValues(request: DataValuesGetPayload) {
		this.tryLoadValues(request)
	}

	private getCacheKey(request: DataValuesGetPayload): string {
		return JSON.stringify(request)
	}

	private valuesCache = observable.map<string, ValuesResponse[]>()

	private async tryLoadValues(request: DataValuesGetPayload): Promise<void> {
		const cacheKey = this.getCacheKey(request)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'ValuesStore',
				method: 'tryLoadValues',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataValuesGet(request)

			if (response.status === 200) {
				runInAction(() => {
					this.valuesCache.set(cacheKey, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load values'), {
				component: 'ValuesStore',
				method: 'tryLoadValues',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает статусы тегов
	 * @param tagsId Массив идентификаторов тегов
	 * @returns Объект со статусами (ключ: tagId)
	 */
	public getStatus(tagsId: number[]): Record<number, string> {
		if (tagsId.length === 0) return {}

		const result: Record<number, string> = {}
		for (const tagId of tagsId) {
			const cached = this.statusCache.get(tagId)
			if (cached) {
				result[tagId] = cached
			}
		}

		this.tryLoadStatus(tagsId)
		return result
	}

	public refreshStatus(tagsId: number[]) {
		this.tryLoadStatus(tagsId)
	}

	private statusCacheKey = 'status'
	private statusCache = observable.map<number, string>()

	private async tryLoadStatus(tagsId: number[]): Promise<void> {
		if (tagsId.length === 0) return

		const cacheKey = this.statusCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'ValuesStore',
				method: 'tryLoadStatus',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataTagsGetStatus({
				tagsId: tagsId,
			})

			if (response.status === 200) {
				runInAction(() => {
					for (const statusInfo of response.data) {
						if (statusInfo.tagId !== undefined) {
							const status = statusInfo.status ?? (statusInfo.isError ? 'Error' : 'Ok')
							this.statusCache.set(statusInfo.tagId, status)
							this.setLastFetchTime(`status_${statusInfo.tagId}`)
						}
					}
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load tags status'), {
				component: 'ValuesStore',
				method: 'tryLoadStatus',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш значений для указанных тегов
	 * @param _tagsId Массив идентификаторов тегов (не используется, но оставлен для совместимости API)
	 */
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	invalidateValues(_tagsId: number[]): void {
		runInAction(() => {
			// Инвалидируем все кэшированные запросы, которые содержат эти теги
			// Это упрощенная версия - в реальности нужно проверять содержимое запросов
			this.valuesCache.clear()
		})
	}

	/**
	 * Инвалидирует кэш статусов для указанных тегов
	 * @param tagsId Массив идентификаторов тегов
	 */
	invalidateStatus(tagsId: number[]): void {
		runInAction(() => {
			for (const tagId of tagsId) {
				this.statusCache.delete(tagId)
			}
		})
	}

	/**
	 * Проверяет, идет ли загрузка для указанного запроса
	 * @param request Запрос на получение значений
	 * @returns true, если идет загрузка
	 */
	isLoadingValues(request: DataValuesGetPayload): boolean {
		const cacheKey = this.getCacheKey(request)
		return this.isLoading(cacheKey)
	}
}
