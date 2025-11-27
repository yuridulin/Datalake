import { Api } from '@/generated/Api'
import { SourceType, TagSimpleInfo, TagWithSettingsAndBlocksInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления тегами с кэшированием последнего успешного ответа
 */
export class TagsStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getTags: true,
			getTagById: true,
		})
	}

	/**
	 * Получает список тегов
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @param sourceId Опциональный идентификатор источника для фильтрации
	 * @returns Список тегов из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTags(sourceId?: SourceType): TagSimpleInfo[] {
		const reactive = this.tagsCache.get(this.getCacheKey(sourceId))
		this.tryLoadTags(sourceId)
		return reactive ?? []
	}

	public refreshTags(sourceId?: SourceType) {
		this.tryLoadTags(sourceId)
	}

	private getCacheKey(sourceId?: SourceType): string {
		return sourceId !== undefined ? `source_${sourceId}` : 'all'
	}

	private tagsCacheKey = (sourceId?: SourceType) => this.getCacheKey(sourceId)
	private tagsCache = observable.map<string, TagSimpleInfo[]>()

	private async tryLoadTags(sourceId?: SourceType): Promise<void> {
		const cacheKey = this.tagsCacheKey(sourceId)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'TagsStore',
				method: 'tryLoadTags',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsGetAll({
				sourceId: sourceId ?? null,
			})

			if (response.status === 200) {
				runInAction(() => {
					this.tagsCache.set(cacheKey, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load tags'), {
				component: 'TagsStore',
				method: 'tryLoadTags',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает конкретный тег по ID
	 * @param id Идентификатор тега
	 * @returns Информация о теге или undefined
	 */
	getTagById(id: number): TagWithSettingsAndBlocksInfo | undefined {
		const reactive = this.tagsByIdCache.get(id)
		this.tryLoadTagById(id)
		return reactive
	}

	private tagsByIdCacheKey = (id: number) => `tag_${id}`
	private tagsByIdCache = observable.map<number, TagWithSettingsAndBlocksInfo>()

	/**
	 * Загружает конкретный тег по ID
	 * @param id Идентификатор тега
	 */
	private async tryLoadTagById(id: number): Promise<void> {
		const cacheKey = this.tagsByIdCacheKey(id)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'TagsStore',
				method: 'tryLoadTagById',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsGetWithSettingsAndBlocks(id)

			if (response.status === 200) {
				runInAction(() => {
					this.tagsByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load tag ${id}`), {
				component: 'TagsStore',
				method: 'tryLoadTagById',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретного тега
	 * @param id Идентификатор тега
	 */
	invalidateTag(id: number): void {
		runInAction(() => {
			this.tagsByIdCache.delete(id)
		})
	}
}
