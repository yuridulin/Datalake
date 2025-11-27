import { Api } from '@/generated/Api'
import { SourceType, TagSimpleInfo, TagWithSettingsAndBlocksInfo } from '@/generated/data-contracts'
import { CACHE_TTL } from '@/config/cacheConfig'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления тегами с кэшированием
 */
export class TagsStore extends BaseCacheStore {
	// Кэш списков тегов (ключ: 'all' | 'source_{id}')
	private _tagsCache = observable.map<string, TagSimpleInfo[]>()

	// Кэш отдельных тегов по ID
	private _tagsByIdCache = observable.map<number, TagWithSettingsAndBlocksInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = CACHE_TTL.TAGS.LIST
	private readonly TTL_ITEM = CACHE_TTL.TAGS.ITEM

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getTags: true,
			getTagById: true,
			isLoadingTags: true,
			isLoadingTag: true,
			invalidateTag: true,
			refreshTags: true,
		})
	}

	/**
	 * Получает список тегов
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @param sourceId Опциональный идентификатор источника для фильтрации
	 * @returns Список тегов из кэша (может быть пустым, пока данные загружаются)
	 */
	getTags(sourceId?: SourceType): TagSimpleInfo[] {
		const cacheKey = this.getCacheKey(sourceId)
		const cached = this._tagsCache.get(cacheKey)

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadTags(sourceId, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'TagsStore',
					method: 'getTags',
					action: 'loadTags',
					sourceId,
				})
			})
		}

		// Возвращаем текущий кэш (может быть пустым)
		return cached ?? []
	}

	/**
	 * Получает конкретный тег по ID
	 * @param id Идентификатор тега
	 * @returns Информация о теге или undefined
	 */
	getTagById(id: number): TagWithSettingsAndBlocksInfo | undefined {
		const cacheKey = `tag_${id}`
		const cached = this._tagsByIdCache.get(id)

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadTagById(id, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'TagsStore',
					method: 'getTagById',
					action: 'loadTagById',
					tagId: id,
				})
			})
		}

		// Возвращаем текущий кэш
		return cached
	}

	/**
	 * Проверяет, идет ли загрузка тегов
	 * @param sourceId Опциональный идентификатор источника
	 * @returns true, если идет загрузка
	 */
	isLoadingTags(sourceId?: SourceType): boolean {
		return this.isLoading(this.getCacheKey(sourceId))
	}

	/**
	 * Проверяет, есть ли данные в кэше для списка тегов
	 * @param sourceId Опциональный идентификатор источника
	 * @returns true, если данные были загружены хотя бы раз
	 */
	hasTagsCache(sourceId?: SourceType): boolean {
		return this.hasCache(this.getCacheKey(sourceId))
	}

	/**
	 * Проверяет, идет ли загрузка конкретного тега
	 * @param id Идентификатор тега
	 * @returns true, если идет загрузка
	 */
	isLoadingTag(id: number): boolean {
		return this.isLoading(`tag_${id}`)
	}

	/**
	 * Инвалидирует кэш для конкретного тега
	 * @param id Идентификатор тега
	 */
	invalidateTag(id: number): void {
		runInAction(() => {
			this._tagsByIdCache.delete(id)
			// Инвалидируем все списки, так как они могут содержать этот тег
			this._tagsCache.forEach((_, key) => {
				this.invalidateCache(key)
			})
		})
	}

	/**
	 * Принудительно обновляет список тегов
	 * @param sourceId Опциональный идентификатор источника
	 */
	async refreshTags(sourceId?: SourceType): Promise<void> {
		// Просто вызываем loadTags - он сам проверит, идет ли загрузка
		await this.loadTags(sourceId, 'manual-refresh')
	}

	/**
	 * Загружает список тегов с сервера
	 * @param sourceId Опциональный идентификатор источника
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadTags(sourceId?: SourceType, reason: string = 'unknown'): Promise<void> {
		const cacheKey = this.getCacheKey(sourceId)

		if (this.isLoading(cacheKey)) {
			logger.debug(`[TagsStore] Skipping loadTags - already loading`, {
				component: 'TagsStore',
				method: 'loadTags',
				cacheKey,
				reason,
			})
			return
		}

		logger.info(`[TagsStore] API Request: inventoryTagsGetAll`, {
			component: 'TagsStore',
			method: 'loadTags',
			cacheKey,
			reason,
			sourceId,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsGetAll({
				sourceId: sourceId ?? null,
			})

			if (response.status === 200) {
				runInAction(() => {
					this._tagsCache.set(cacheKey, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load tags'), {
				component: 'TagsStore',
				method: 'loadTags',
				sourceId,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретный тег по ID
	 * @param id Идентификатор тега
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadTagById(id: number, reason: string = 'unknown'): Promise<void> {
		const cacheKey = `tag_${id}`

		if (this.isLoading(cacheKey)) {
			logger.debug(`[TagsStore] Skipping loadTagById - already loading`, {
				component: 'TagsStore',
				method: 'loadTagById',
				cacheKey,
				reason,
				tagId: id,
			})
			return
		}

		logger.info(`[TagsStore] API Request: inventoryTagsGetWithSettingsAndBlocks`, {
			component: 'TagsStore',
			method: 'loadTagById',
			cacheKey,
			reason,
			tagId: id,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsGetWithSettingsAndBlocks(id)

			if (response.status === 200) {
				runInAction(() => {
					this._tagsByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load tag ${id}`), {
				component: 'TagsStore',
				method: 'loadTagById',
				tagId: id,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Генерирует ключ кэша для списка тегов
	 * @param sourceId Опциональный идентификатор источника
	 * @returns Ключ кэша
	 */
	private getCacheKey(sourceId?: SourceType): string {
		return sourceId !== undefined ? `source_${sourceId}` : 'all'
	}
}
