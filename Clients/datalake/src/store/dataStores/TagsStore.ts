// TagsStore.ts
import { Api } from '@/generated/Api'
import {
	SourceType,
	TagCreateRequest,
	TagSimpleInfo,
	TagUpdateRequest,
	TagWithSettingsAndBlocksInfo,
} from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { computed, makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

export class TagsStore extends BaseCacheStore {
	// Observable данные
	private tagsCache = observable.box<TagSimpleInfo[]>([])
	private tagsByIdCache = observable.map<number, TagWithSettingsAndBlocksInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			agregatedTags: computed,
			calculatedTags: computed,
			manualTags: computed,
			thresholdsTags: computed,
		})
	}

	//#region Методы получения

	/**
	 * Получает список тегов
	 * @param sourceId Опциональный идентификатор источника для фильтрации
	 * @returns Список тегов из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTags(): TagSimpleInfo[] {
		return this.tagsCache.get()
	}

	/**
	 * Получает конкретный тег по ID
	 * @param id Идентификатор тега
	 * @returns Информация о теге или undefined
	 */
	public getTagById(id: number): TagWithSettingsAndBlocksInfo | undefined {
		return this.tagsByIdCache.get(id)
	}

	//#endregion

	//#region Computed свойства

	/**
	 * Список мануальных тегов
	 */
	get manualTags(): TagSimpleInfo[] {
		const tags = this.tagsCache.get()
		return tags.filter((x) => x.sourceType === SourceType.Manual)
	}

	/**
	 * Список вычисляемых тегов
	 */
	get calculatedTags(): TagSimpleInfo[] {
		const tags = this.tagsCache.get()
		return tags.filter((x) => x.sourceType === SourceType.Calculated)
	}

	/**
	 * Список агрегатных тегов
	 */
	get agregatedTags(): TagSimpleInfo[] {
		const tags = this.tagsCache.get()
		return tags.filter((x) => x.sourceType === SourceType.Aggregated)
	}

	/**
	 * Список пороговых тегов
	 */
	get thresholdsTags(): TagSimpleInfo[] {
		const tags = this.tagsCache.get()
		return tags.filter((x) => x.sourceType === SourceType.Thresholds)
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка тегов
	 */
	public refreshTags() {
		this.tryLoadTags()
	}

	/**
	 * Сигнал обновления конкретного тега
	 */
	public refreshTagById(id: number) {
		this.tryLoadTagById(id)
	}

	/**
	 * Создание нового тега
	 */
	public async createTag(data: TagCreateRequest): Promise<TagSimpleInfo | undefined> {
		const cacheKey = 'creating-tag'

		if (this.isLoading(cacheKey)) return undefined
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsCreate(data)

			runInAction(() => {
				logger.info('Tag created successfully', {
					component: 'TagsStore',
					method: 'createTag',
					tagId: response.data?.id,
				})
				this.tryLoadTags()
			})

			return response.data
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create tag'), {
				component: 'TagsStore',
				method: 'createTag',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Обновление тега
	 */
	public async updateTag(id: number, data: TagUpdateRequest): Promise<void> {
		const cacheKey = `updating-tag-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryTagsUpdate(id, data)

			runInAction(() => {
				logger.info('Tag updated successfully', {
					component: 'TagsStore',
					method: 'updateTag',
					tagId: id,
				})

				this.refreshTagById(id)
				this.refreshTags()
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update tag'), {
				component: 'TagsStore',
				method: 'updateTag',
				tagId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Удаление тега
	 */
	public async deleteTag(id: number): Promise<void> {
		const cacheKey = `deleting-tag-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryTagsDelete(id)

			runInAction(() => {
				logger.info('Tag deleted successfully', {
					component: 'TagsStore',
					method: 'deleteTag',
					tagId: id,
				})

				this.refreshTags()
				this.tagsByIdCache.delete(id)
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete tag'), {
				component: 'TagsStore',
				method: 'deleteTag',
				tagId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion

	//#region Приватные методы загрузки

	private async tryLoadTags(): Promise<void> {
		const cacheKey = 'tags'

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryTagsGetAll()

			if (response.status === 200) {
				runInAction(() => {
					this.tagsCache.set(response.data)
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

	private async tryLoadTagById(id: number): Promise<void> {
		const cacheKey = `tag_${id}`

		if (this.isLoading(cacheKey)) return
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
			logger.error(error instanceof Error ? error : new Error('Failed to load tag by id'), {
				component: 'TagsStore',
				method: 'tryLoadTagById',
				tagId: id,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion
}
