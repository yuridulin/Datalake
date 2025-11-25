import { Api } from '@/generated/Api'
import { BlockDetailedInfo, BlockTreeInfo, BlockWithTagsInfo } from '@/generated/data-contracts'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления блоками с кэшированием
 */
export class BlocksStore extends BaseCacheStore {
	// Кэш всех блоков (observable для реактивности)
	private _blocksCache = observable.box<BlockWithTagsInfo[] | null>(null)

	// Кэш дерева блоков (observable для реактивности)
	private _blocksTreeCache = observable.box<BlockTreeInfo[] | null>(null)

	// Кэш отдельных блоков по ID (observable Map)
	private _blocksByIdCache = observable.map<number, BlockDetailedInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = 1 * 60 * 1000 // 1 минута для списка (обновляется через polling)
	private readonly TTL_TREE = 1 * 60 * 1000 // 1 минута для дерева
	private readonly TTL_ITEM = 5 * 60 * 1000 // 5 минут для отдельного блока

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getBlocks: true,
			getTree: true,
			getBlockById: true,
			isLoadingBlocks: true,
			invalidateBlock: true,
			refreshBlocks: true,
			refreshTree: true,
		})
	}

	/**
	 * Получает список всех блоков
	 * @returns Список блоков
	 */
	getBlocks(): BlockWithTagsInfo[] {
		const cacheKey = 'blocks_list'
		const cached = this._blocksCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadBlocks().catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadBlocks().catch(console.error)
		}

		return cached ?? []
	}

	/**
	 * Получает дерево блоков
	 * @returns Дерево блоков
	 */
	getTree(): BlockTreeInfo[] {
		const cacheKey = 'blocks_tree'
		const cached = this._blocksTreeCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_TREE)) {
			if (this.shouldRefresh(cacheKey, this.TTL_TREE)) {
				this.loadTree().catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadTree().catch(console.error)
		}

		return cached ?? []
	}

	/**
	 * Получает конкретный блок по ID
	 * @param id Идентификатор блока
	 * @returns Информация о блоке или undefined
	 */
	getBlockById(id: number): BlockDetailedInfo | undefined {
		const cacheKey = `block_${id}`
		const cached = this._blocksByIdCache.get(id)

		if (cached && this.isCacheValid(cacheKey, this.TTL_ITEM)) {
			if (this.shouldRefresh(cacheKey, this.TTL_ITEM)) {
				this.loadBlockById(id).catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadBlockById(id).catch(console.error)
		}

		return cached
	}

	/**
	 * Проверяет, идет ли загрузка блоков
	 * @returns true, если идет загрузка
	 */
	isLoadingBlocks(): boolean {
		return this.isLoading('blocks_list')
	}

	/**
	 * Инвалидирует кэш для конкретного блока
	 * @param id Идентификатор блока
	 */
	invalidateBlock(id: number): void {
		runInAction(() => {
			this._blocksByIdCache.delete(id)
			// Инвалидируем списки, так как они могут содержать этот блок
			this.invalidateCache('blocks_list')
			this.invalidateCache('blocks_tree')
		})
	}

	/**
	 * Принудительно обновляет список блоков
	 */
	async refreshBlocks(): Promise<void> {
		this.invalidateCache('blocks_list')
		await this.loadBlocks()
	}

	/**
	 * Принудительно обновляет дерево блоков
	 */
	async refreshTree(): Promise<void> {
		this.invalidateCache('blocks_tree')
		await this.loadTree()
	}

	/**
	 * Загружает список блоков с сервера
	 */
	private async loadBlocks(): Promise<void> {
		const cacheKey = 'blocks_list'

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGetAll()

			if (response.status === 200) {
				runInAction(() => {
					this._blocksCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error('Failed to load blocks:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает дерево блоков с сервера
	 */
	private async loadTree(): Promise<void> {
		const cacheKey = 'blocks_tree'

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGetTree()

			if (response.status === 200) {
				runInAction(() => {
					this._blocksTreeCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error('Failed to load blocks tree:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретный блок по ID
	 * @param id Идентификатор блока
	 */
	private async loadBlockById(id: number): Promise<void> {
		const cacheKey = `block_${id}`

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGet(id)

			if (response.status === 200) {
				runInAction(() => {
					this._blocksByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error(`Failed to load block ${id}:`, error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
