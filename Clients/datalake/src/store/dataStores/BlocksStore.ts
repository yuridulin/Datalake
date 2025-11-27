import { Api } from '@/generated/Api'
import { BlockDetailedInfo, BlockTreeInfo, BlockWithTagsInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления блоками с кэшированием последнего успешного ответа
 */
export class BlocksStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getBlocks: true,
			getTree: true,
			getBlockById: true,
		})
	}

	/**
	 * Получает список всех блоков
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список блоков из кэша (может быть пустым, пока данные загружаются)
	 */
	public getBlocks(): BlockWithTagsInfo[] {
		return this.blocksCache.get()
	}

	public refreshBlocks() {
		this.tryLoadBlocks()
	}

	private blocksCacheKey = 'blocks'

	private blocksCache = observable.box<BlockWithTagsInfo[]>([])

	private async tryLoadBlocks(): Promise<void> {
		const cacheKey = this.blocksCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'BlocksStore',
				method: 'tryLoadBlocks',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGetAll()

			if (response.status === 200) {
				runInAction(() => {
					this.blocksCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load blocks'), {
				component: 'BlocksStore',
				method: 'tryLoadBlocks',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает дерево блоков
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Дерево блоков из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTree(): BlockTreeInfo[] {
		const reactive = this.blocksTreeCache.get()
		this.tryLoadBlocksTree()
		return reactive
	}

	private blocksTreeCacheKey = 'blocks-tree'
	private blocksTreeCache = observable.box<BlockTreeInfo[]>([])

	private async tryLoadBlocksTree(): Promise<void> {
		const cacheKey = this.blocksTreeCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'BlocksStore',
				method: 'tryLoadBlocksTree',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGetTree()

			if (response.status === 200) {
				runInAction(() => {
					this.blocksTreeCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load blocks tree'), {
				component: 'BlocksStore',
				method: 'tryLoadBlocksTree',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает конкретный блок по ID
	 * @param id Идентификатор блока
	 * @returns Информация о блоке или undefined
	 */
	getBlockById(id: number): BlockDetailedInfo | undefined {
		const reactive = this.blocksByIdCache.get(id)
		this.tryLoadBlocksById(id)
		return reactive
	}

	private blocksByIdCacheKey = (id: number) => `blocks-detailed:${id}`
	private blocksByIdCache = observable.map<number, BlockDetailedInfo>()

	/**
	 * Загружает конкретный блок по ID
	 * @param id Идентификатор блока
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async tryLoadBlocksById(id: number): Promise<void> {
		const cacheKey = this.blocksByIdCacheKey(id)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'BlocksStore',
				method: 'blocksByIdCacheKey',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGetTree()

			if (response.status === 200) {
				runInAction(() => {
					this.blocksTreeCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load block by id'), {
				component: 'BlocksStore',
				method: 'blocksByIdCacheKey',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
