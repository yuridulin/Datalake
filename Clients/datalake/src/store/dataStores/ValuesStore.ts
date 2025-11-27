// ValuesStore.ts
import { Api } from '@/generated/Api'
import { DataValuesGetPayload, ValuesResponse } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { AxiosError } from 'axios'
import { observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления значениями тегов
 * Особенности:
 * - Нет кэширования значений (каждый запрос возвращает актуальные данные)
 * - При новом запросе предыдущий запрос отменяется
 */
export class ValuesStore extends BaseCacheStore {
	// Observable данные
	// Текущие значения для активного запроса (без кэширования)
	private currentValues = observable.box<ValuesResponse[]>([])
	// Текущий активный запрос
	private currentRequest: DataValuesGetPayload | null = null
	// AbortController для отмены текущего запроса
	private currentAbortController: AbortController | null = null

	// Кэш для статусов (статусы можно кэшировать)
	private statusCache = observable.map<number, string>()

	constructor(private api: Api) {
		super()
		// currentValues уже observable через observable.box, не нужно указывать в makeObservable
	}

	//#region Методы получения значений

	/**
	 * Получает значения тегов по запросу
	 * При новом запросе отменяет предыдущий запрос
	 * @param request Запрос на получение значений
	 * @returns Массив ответов с значениями (из текущего запроса или пустой массив)
	 */
	public getValues(request: DataValuesGetPayload): ValuesResponse[] {
		// Если запрос изменился, отменяем предыдущий и запускаем новый
		if (!this.isSameRequest(this.currentRequest, request)) {
			this.cancelCurrentRequest()
			// Очищаем значения при новом запросе (нет кэширования)
			runInAction(() => {
				this.currentValues.set([])
			})
			this.currentRequest = request
			this.tryLoadValues(request)
		}

		return this.currentValues.get()
	}

	/**
	 * Принудительное обновление значений
	 * Отменяет текущий запрос и запускает новый
	 * @param request Запрос на получение значений
	 */
	public refreshValues(request: DataValuesGetPayload) {
		this.cancelCurrentRequest()
		this.currentRequest = request
		this.tryLoadValues(request)
	}

	/**
	 * Проверяет, идет ли загрузка для указанного запроса
	 * @param request Запрос на получение значений
	 * @returns true, если идет загрузка
	 */
	public isLoadingValues(request: DataValuesGetPayload): boolean {
		const cacheKey = this.getCacheKey(request)
		return this.isLoading(cacheKey)
	}

	//#endregion

	//#region Методы получения статусов

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

	/**
	 * Принудительное обновление статусов
	 * @param tagsId Массив идентификаторов тегов
	 */
	public refreshStatus(tagsId: number[]) {
		this.tryLoadStatus(tagsId)
	}

	/**
	 * Инвалидирует кэш статусов для указанных тегов
	 * @param tagsId Массив идентификаторов тегов
	 */
	public invalidateStatus(tagsId: number[]): void {
		runInAction(() => {
			for (const tagId of tagsId) {
				this.statusCache.delete(tagId)
			}
		})
	}

	//#endregion

	//#region Приватные методы загрузки

	/**
	 * Загружает значения тегов
	 * При новом запросе отменяет предыдущий
	 */
	private async tryLoadValues(request: DataValuesGetPayload): Promise<void> {
		const cacheKey = this.getCacheKey(request)

		// Создаем новый AbortController для этого запроса
		const abortController = new AbortController()
		this.currentAbortController = abortController

		this.setLoading(cacheKey, true)

		try {
			// Передаем signal в запрос через params (Axios поддерживает signal)
			const response = await this.api.dataValuesGet(request, {
				signal: abortController.signal,
			} as { signal?: AbortSignal })

			// Проверяем, не был ли запрос отменен
			if (abortController.signal.aborted) {
				return
			}

			if (response.status === 200) {
				runInAction(() => {
					// Обновляем текущие значения только если это все еще актуальный запрос
					if (this.isSameRequest(this.currentRequest, request)) {
						this.currentValues.set(response.data)
					}
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error: unknown) {
			// Игнорируем ошибки отмены запроса
			if (error instanceof Error && (error.name === 'AbortError' || error.name === 'CanceledError')) {
				logger.debug('Request cancelled', {
					component: 'ValuesStore',
					method: 'tryLoadValues',
				})
				return
			}

			// Проверяем, является ли ошибка AxiosError с кодом отмены
			if (error instanceof AxiosError && error.code === 'ERR_CANCELED') {
				logger.debug('Request cancelled (Axios)', {
					component: 'ValuesStore',
					method: 'tryLoadValues',
				})
				return
			}

			logger.error(error instanceof Error ? error : new Error('Failed to load values'), {
				component: 'ValuesStore',
				method: 'tryLoadValues',
			})
		} finally {
			// Сбрасываем состояние загрузки только если это все еще актуальный запрос
			if (this.isSameRequest(this.currentRequest, request)) {
				this.setLoading(cacheKey, false)
			}
		}
	}

	/**
	 * Загружает статусы тегов
	 */
	private async tryLoadStatus(tagsId: number[]): Promise<void> {
		if (tagsId.length === 0) return

		const cacheKey = 'status'
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

	//#endregion

	//#region Вспомогательные методы

	/**
	 * Отменяет текущий запрос значений
	 */
	private cancelCurrentRequest(): void {
		if (this.currentAbortController) {
			this.currentAbortController.abort()
			this.currentAbortController = null
		}
		this.currentRequest = null
	}

	/**
	 * Сравнивает два запроса на равенство
	 */
	private isSameRequest(request1: DataValuesGetPayload | null, request2: DataValuesGetPayload | null): boolean {
		if (request1 === null || request2 === null) {
			return request1 === request2
		}

		// Сравниваем по JSON строке для простоты
		return JSON.stringify(request1) === JSON.stringify(request2)
	}

	/**
	 * Генерирует ключ кэша для запроса (используется только для состояния загрузки)
	 */
	private getCacheKey(request: DataValuesGetPayload): string {
		return JSON.stringify(request)
	}

	/**
	 * Инвалидирует значения (для совместимости с API, но не делает ничего, так как кэша нет)
	 * @param _tagsId Массив идентификаторов тегов (не используется)
	 */
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public invalidateValues(_tagsId: number[]): void {
		// Нет кэша, поэтому ничего не делаем
		// Можно очистить текущие значения, если нужно
		runInAction(() => {
			this.currentValues.set([])
			this.currentRequest = null
		})
	}

	//#endregion
}
