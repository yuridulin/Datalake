import { Api } from '@/generated/Api'
import { DataValuesGetPayload, ValuesResponse } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления значениями тегов с кэшированием
 * Особенности: короткий TTL из-за частых обновлений, поддержка polling
 */
export class ValuesStore extends BaseCacheStore {
	// Кэш значений по ключу запроса (JSON.stringify запроса)
	private _valuesCache = observable.map<string, ValuesResponse[]>()

	// Кэш статусов тегов (ключ: tagId)
	private _statusCache = observable.map<number, string>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_VALUES = 30 * 1000 // 30 секунд для значений
	private readonly TTL_STATUS = 10 * 1000 // 10 секунд для статусов

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getValues: true,
			getStatus: true,
			invalidateValues: true,
			invalidateStatus: true,
			refreshValues: true,
			refreshStatus: true,
			isLoadingValues: true,
		})
	}

	/**
	 * Получает значения тегов по запросу
	 * @param request Запрос на получение значений
	 * @returns Массив ответов с значениями
	 */
	getValues(request: DataValuesGetPayload): ValuesResponse[] {
		const cacheKey = this.getCacheKey(request)
		const cached = this._valuesCache.get(cacheKey)

		if (cached && this.isCacheValid(cacheKey, this.TTL_VALUES)) {
			if (this.shouldRefresh(cacheKey, this.TTL_VALUES)) {
				this.loadValues(request).catch((error) => {
					logger.error(error instanceof Error ? error : new Error(String(error)), {
						component: 'ValuesStore',
						method: 'getValues',
						action: 'loadValues',
					})
				})
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadValues(request).catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'ValuesStore',
					method: 'getValues',
					action: 'loadValues',
				})
			})
		}

		return cached ?? []
	}

	/**
	 * Получает статусы тегов
	 * @param tagsId Массив идентификаторов тегов
	 * @returns Объект со статусами (ключ: tagId)
	 */
	getStatus(tagsId: number[]): Record<number, string> {
		if (tagsId.length === 0) return {}

		const result: Record<number, string> = {}
		const needRefresh: number[] = []

		// Проверяем кэш для каждого тега
		for (const tagId of tagsId) {
			const cacheKey = `status_${tagId}`
			const cached = this._statusCache.get(tagId)

			if (cached && this.isCacheValid(cacheKey, this.TTL_STATUS)) {
				result[tagId] = cached
				if (this.shouldRefresh(cacheKey, this.TTL_STATUS)) {
					needRefresh.push(tagId)
				}
			} else {
				needRefresh.push(tagId)
			}
		}

		// Обновляем в фоне, если нужно
		if (needRefresh.length > 0 && !this.isLoading('status')) {
			this.loadStatus(needRefresh).catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'ValuesStore',
					method: 'getStatus',
					action: 'loadStatus',
					tagsId: needRefresh,
				})
			})
		}

		return result
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
			this._valuesCache.clear()
		})
	}

	/**
	 * Инвалидирует кэш статусов для указанных тегов
	 * @param tagsId Массив идентификаторов тегов
	 */
	invalidateStatus(tagsId: number[]): void {
		runInAction(() => {
			for (const tagId of tagsId) {
				this._statusCache.delete(tagId)
				this.invalidateCache(`status_${tagId}`)
			}
		})
	}

	/**
	 * Принудительно обновляет значения
	 * @param request Запрос на получение значений
	 */
	async refreshValues(request: DataValuesGetPayload): Promise<void> {
		const cacheKey = this.getCacheKey(request)
		this.invalidateCache(cacheKey)
		await this.loadValues(request)
	}

	/**
	 * Принудительно обновляет статусы тегов
	 * @param tagsId Массив идентификаторов тегов
	 */
	async refreshStatus(tagsId: number[]): Promise<void> {
		for (const tagId of tagsId) {
			this.invalidateCache(`status_${tagId}`)
		}
		await this.loadStatus(tagsId)
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

	/**
	 * Загружает значения с сервера
	 * @param request Запрос на получение значений
	 */
	private async loadValues(request: DataValuesGetPayload): Promise<void> {
		const cacheKey = this.getCacheKey(request)

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataValuesGet(request)

			if (response.status === 200) {
				runInAction(() => {
					this._valuesCache.set(cacheKey, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load values'), {
				component: 'ValuesStore',
				method: 'loadValues',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает статусы тегов с сервера
	 * @param tagsId Массив идентификаторов тегов
	 */
	private async loadStatus(tagsId: number[]): Promise<void> {
		if (tagsId.length === 0) return

		const cacheKey = 'status'

		if (this.isLoading(cacheKey)) return

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
							this._statusCache.set(statusInfo.tagId, status)
							this.setLastFetchTime(`status_${statusInfo.tagId}`)
						}
					}
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load tags status'), {
				component: 'ValuesStore',
				method: 'loadStatus',
				tagsId: tagsId,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Генерирует ключ кэша для запроса значений
	 * @param request Запрос на получение значений
	 * @returns Ключ кэша
	 */
	getCacheKey(request: DataValuesGetPayload): string {
		// Создаем уникальный ключ на основе содержимого запроса
		return JSON.stringify(request)
	}
}
