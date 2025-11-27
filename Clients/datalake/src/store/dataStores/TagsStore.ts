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
	private tagsCache = observable.map<string, TagSimpleInfo[]>()
	private tagsByIdCache = observable.map<number, TagWithSettingsAndBlocksInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			// Computed свойства
			statistics: computed,
			tagsBySource: computed,
		})
	}

	//#region Методы получения

	/**
	 * Получает список тегов
	 * @param sourceId Опциональный идентификатор источника для фильтрации
	 * @returns Список тегов из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTags(sourceId?: SourceType): TagSimpleInfo[] {
		const cacheKey = this.getCacheKey(sourceId)
		return this.tagsCache.get(cacheKey) ?? []
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
	 * Группировка тегов по источникам
	 */
	get tagsBySource(): Map<SourceType, TagSimpleInfo[]> {
		return computed(() => {
			const allTags = this.tagsCache.get(this.getCacheKey()) ?? []
			const grouped = new Map<SourceType, TagSimpleInfo[]>()

			allTags.forEach((tag) => {
				const sourceId = tag.sourceId ?? SourceType.Unset
				if (!grouped.has(sourceId)) {
					grouped.set(sourceId, [])
				}
				grouped.get(sourceId)!.push(tag)
			})

			return grouped
		}).get()
	}

	/**
	 * Статистика по тегам
	 */
	get statistics() {
		return computed(() => {
			const allTags = this.tagsCache.get(this.getCacheKey()) ?? []

			const bySource = new Map<SourceType, number>()
			const byType = new Map<string, number>()

			allTags.forEach((tag) => {
				const sourceId = tag.sourceId ?? SourceType.Unset
				bySource.set(sourceId, (bySource.get(sourceId) ?? 0) + 1)

				const tagType = tag.type?.toString() ?? 'unknown'
				byType.set(tagType, (byType.get(tagType) ?? 0) + 1)
			})

			return {
				totalTags: allTags.length,
				bySource: Object.fromEntries(bySource),
				byType: Object.fromEntries(byType),
				manualTags: allTags.filter((t) => t.sourceId === SourceType.Manual).length,
				calculatedTags: allTags.filter((t) => t.sourceId === SourceType.Calculated).length,
				aggregatedTags: allTags.filter((t) => t.sourceId === SourceType.Aggregated).length,
			}
		}).get()
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка тегов
	 */
	public refreshTags(sourceId?: SourceType) {
		this.tryLoadTags(sourceId)
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

				// Инвалидируем кэш для соответствующего источника
				if (data.sourceId !== undefined) {
					this.tagsCache.delete(this.getCacheKey(data.sourceId))
				}
				this.tagsCache.delete(this.getCacheKey()) // Инвалидируем общий кэш
				this.tryLoadTags(data.sourceId)
				this.tryLoadTags() // Обновляем общий список
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

				// Инвалидируем кэши
				this.tagsByIdCache.delete(id)
				this.tagsCache.delete(this.getCacheKey()) // Инвалидируем общий кэш
				// Инвалидируем все кэши по источникам, так как источник тега мог измениться
				this.tagsCache.forEach((_, key) => {
					if (key !== this.getCacheKey()) {
						this.tagsCache.delete(key)
					}
				})

				this.refreshTagById(id)
				this.refreshTags() // Обновляем общий список
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

				// Удаляем из кэшей
				this.tagsByIdCache.delete(id)
				// Удаляем из всех списков тегов
				this.tagsCache.forEach((tags, key) => {
					const filtered = tags.filter((t) => t.id !== id)
					if (filtered.length !== tags.length) {
						this.tagsCache.set(key, filtered)
					}
				})

				this.refreshTags() // Обновляем общий список
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

	/**
	 * Инвалидирует кэш для конкретного тега
	 * @param id Идентификатор тега
	 */
	public invalidateTag(id: number): void {
		runInAction(() => {
			this.tagsByIdCache.delete(id)
		})
	}

	/**
	 * Проверяет, идет ли загрузка списка тегов
	 * @param sourceId Опциональный идентификатор источника
	 */
	public isLoadingTags(sourceId?: SourceType): boolean {
		return this.isLoading(this.getCacheKey(sourceId))
	}

	/**
	 * Проверяет, идет ли загрузка конкретного тега
	 * @param id Идентификатор тега
	 */
	public isLoadingTag(id: number): boolean {
		return this.isLoading(`tag_${id}`)
	}

	//#endregion

	//#region Приватные методы загрузки

	private getCacheKey(sourceId?: number | null): string {
		return sourceId ? `source_${sourceId}` : 'all'
	}

	private async tryLoadTags(sourceId?: number | null): Promise<void> {
		const cacheKey = this.getCacheKey(sourceId)

		if (this.isLoading(cacheKey)) return
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
