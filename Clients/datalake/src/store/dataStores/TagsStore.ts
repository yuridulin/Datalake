import { Api } from '@/generated/Api'
import { SourceType, TagSimpleInfo, TagWithSettingsAndBlocksInfo } from '@/generated/data-contracts'
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
	private readonly TTL_LIST = 5 * 60 * 1000 // 5 минут для списков
	private readonly TTL_ITEM = 10 * 60 * 1000 // 10 минут для отдельного тега

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
	 * Получает список тегов с stale-while-revalidate паттерном
	 * @param sourceId Опциональный идентификатор источника для фильтрации
	 * @returns Список тегов (может быть из кэша, если данные устарели)
	 */
	getTags(sourceId?: SourceType): TagSimpleInfo[] {
		const cacheKey = this.getCacheKey(sourceId)
		const cached = this._tagsCache.get(cacheKey)

		// Если кэш валиден, возвращаем его
		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			// Если кэш устарел на 80%, обновляем в фоне
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadTags(sourceId).catch((error) => {
					logger.error(error instanceof Error ? error : new Error(String(error)), {
						component: 'TagsStore',
						method: 'getTags',
						action: 'loadTags',
						sourceId,
					})
				})
			}
			return cached
		}

		// Если кэша нет или он невалиден, загружаем синхронно
		if (!this.isLoading(cacheKey)) {
			this.loadTags(sourceId).catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'TagsStore',
					method: 'getTags',
					action: 'loadTags',
					sourceId,
				})
			})
		}

		// Возвращаем устаревший кэш, если есть, иначе пустой массив
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

		if (cached && this.isCacheValid(cacheKey, this.TTL_ITEM)) {
			if (this.shouldRefresh(cacheKey, this.TTL_ITEM)) {
				this.loadTagById(id).catch((error) => {
					logger.error(error instanceof Error ? error : new Error(String(error)), {
						component: 'TagsStore',
						method: 'getTagById',
						action: 'loadTagById',
						tagId: id,
					})
				})
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadTagById(id).catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'TagsStore',
					method: 'getTagById',
					action: 'loadTagById',
					tagId: id,
				})
			})
		}

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
		const cacheKey = this.getCacheKey(sourceId)
		this.invalidateCache(cacheKey)
		await this.loadTags(sourceId)
	}

	/**
	 * Загружает список тегов с сервера
	 * @param sourceId Опциональный идентификатор источника
	 */
	private async loadTags(sourceId?: SourceType): Promise<void> {
		const cacheKey = this.getCacheKey(sourceId)

		if (this.isLoading(cacheKey)) return

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
	 */
	private async loadTagById(id: number): Promise<void> {
		const cacheKey = `tag_${id}`

		if (this.isLoading(cacheKey)) return

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
