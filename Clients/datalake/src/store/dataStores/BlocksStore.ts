import { Api } from '@/generated/Api'
import { BlockDetailedInfo, BlockTreeInfo, BlockUpdateRequest, BlockWithTagsInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { computed, makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

export class BlocksStore extends BaseCacheStore {
	// Observable данные
	private blocksCache = observable.box<BlockWithTagsInfo[]>([])
	private blocksByIdCache = observable.map<number, BlockDetailedInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			// Computed свойства
			tree: computed,
			flatTreeMap: computed,
		})
	}

	//#region Методы получения

	public getDetailedById(id: number): BlockDetailedInfo | undefined {
		return this.blocksByIdCache.get(id)
	}

	//#endregion

	//#region Computed свойства

	/**
	 * Дерево блоков - автоматически строится из плоского списка
	 */
	get tree(): BlockTreeInfo[] {
		const blocks = this.blocksCache.get()
		return this.buildTree(blocks)
	}

	/**
	 * Плоский маппинг ID -> полный путь в дереве
	 */
	get flatTreeMap(): Map<number, string> {
		const map = new Map<number, string>()

		const buildPaths = (nodes: BlockTreeInfo[], parentPath: string = '') => {
			nodes.forEach((node) => {
				const fullPath = parentPath ? `${parentPath} > ${node.name}` : node.name
				map.set(node.id, fullPath)

				if (node.children && node.children.length > 0) {
					buildPaths(node.children, fullPath)
				}
			})
		}

		buildPaths(this.tree)
		return map
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка блоков
	 */
	public refreshBlocks() {
		this.tryLoad()
	}

	/**
	 * Сигнал обновления конкретного блока
	 */
	public refreshDetailedById(id: number) {
		this.tryLoadDetailedById(id)
	}

	/**
	 * Создание нового блока
	 */
	public async createBlock(data: Partial<{ parentId: number }> = {}): Promise<void> {
		const cacheKey = 'creating-block'

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksCreate(data)

			runInAction(() => {
				logger.info('Block created successfully', {
					component: 'BlocksStore',
					method: 'createBlock',
					blockId: response.data,
				})

				this.tryLoad()
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create block'), {
				component: 'BlocksStore',
				method: 'createBlock',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Обновление блока
	 */
	public async updateBlock(id: number, data: BlockUpdateRequest): Promise<void> {
		const cacheKey = `updating-block-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryBlocksUpdate(id, data)

			runInAction(() => {
				logger.info('Block updated successfully', {
					component: 'BlocksStore',
					method: 'updateBlock',
					blockId: id,
				})

				this.refreshBlocks()
				this.refreshDetailedById(id)
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update block'), {
				component: 'BlocksStore',
				method: 'updateBlock',
				blockId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Удаление блока
	 */
	public async deleteBlock(id: number): Promise<void> {
		const cacheKey = `deleting-block-${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryBlocksDelete(id)

			runInAction(() => {
				logger.info('Block deleted successfully', {
					component: 'BlocksStore',
					method: 'deleteBlock',
					blockId: id,
				})

				this.refreshBlocks()
				this.blocksByIdCache.delete(id) // удаляем кэшированные данные
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete block'), {
				component: 'BlocksStore',
				method: 'deleteBlock',
				blockId: id,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Перемещение блока
	 */
	public async moveBlock(blockId: number, newParentId: number | null): Promise<void> {
		const cacheKey = `moving-block-${blockId}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryBlocksMove(blockId, { parentId: newParentId })
			runInAction(() => {
				this.refreshBlocks()
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion

	//#region Приватные методы загрузки

	private async tryLoad(): Promise<void> {
		const cacheKey = 'blocks'

		if (this.isLoading(cacheKey)) return
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
				method: 'tryLoad',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	private async tryLoadDetailedById(id: number): Promise<void> {
		const cacheKey = `blocks-detailed:${id}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryBlocksGet(id)
			if (response.status === 200) {
				runInAction(() => {
					this.blocksByIdCache.set(id, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load block by id'), {
				component: 'BlocksStore',
				method: 'tryLoadBlocksById',
				blockId: id,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion

	//#region Вспомогательные методы

	/**
	 * Построение дерева из плоского списка
	 */
	private buildTree(blocks: BlockWithTagsInfo[]): BlockTreeInfo[] {
		if (!blocks || blocks.length === 0) return []

		const blockMap = new Map<number, BlockTreeInfo>()
		const roots: BlockTreeInfo[] = []

		// Создаем узлы
		blocks.forEach((block) => {
			blockMap.set(block.id, {
				...block,
				children: [],
			} as BlockTreeInfo)
		})

		// Строим иерархию
		blocks.forEach((block) => {
			const node = blockMap.get(block.id)!

			if (block.parentBlockId && blockMap.has(block.parentBlockId)) {
				const parent = blockMap.get(block.parentBlockId)!
				parent.children!.push(node)
			} else {
				roots.push(node)
			}
		})

		// Сортируем
		roots.forEach((root) => this.sortChildren(root))
		return roots.sort((a, b) => a.name.localeCompare(b.name))
	}

	private sortChildren(node: BlockTreeInfo) {
		if (node.children && node.children.length > 0) {
			node.children.sort((a, b) => a.name.localeCompare(b.name))
			node.children.forEach((child) => this.sortChildren(child))
		}
	}

	/**
	 * Поиск по дереву
	 */
	public searchBlocks(searchTerm: string): BlockTreeInfo[] {
		if (!searchTerm) return this.tree

		const searchLower = searchTerm.toLowerCase()

		const filterTree = (nodes: BlockTreeInfo[]): BlockTreeInfo[] => {
			return nodes
				.map((node) => {
					const matches = node.name.toLowerCase().includes(searchLower)
					const children = node.children ? filterTree(node.children) : []

					if (matches || children.length > 0) {
						return {
							...node,
							children: children.length > 0 ? children : [],
						}
					}
					return null
				})
				.filter(Boolean) as BlockTreeInfo[]
		}

		return filterTree(this.tree)
	}

	//#endregion
}
