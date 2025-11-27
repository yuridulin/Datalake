// UserGroupsStore.ts
import { Api } from '@/generated/Api'
import { UserGroupCreateRequest, UserGroupDetailedInfo, UserGroupInfo, UserGroupTreeInfo, UserGroupUpdateRequest } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { computed, makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

export class UserGroupsStore extends BaseCacheStore {
	// Observable данные
	private groupsCache = observable.box<UserGroupInfo[]>([])
	private groupsTreeCache = observable.box<UserGroupTreeInfo[]>([])
	private groupsByGuidCache = observable.map<string, UserGroupDetailedInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			// Computed свойства
			statistics: computed,
			flatTreeMap: computed,
			groupsMap: computed,
		})
	}

	//#region Методы получения

	/**
	 * Получает список всех групп
	 * @returns Список групп из кэша (может быть пустым, пока данные загружаются)
	 */
	public getGroups(): UserGroupInfo[] {
		return this.groupsCache.get()
	}

	/**
	 * Получает дерево групп
	 * @returns Дерево групп из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTree(): UserGroupTreeInfo[] {
		return this.groupsTreeCache.get()
	}

	/**
	 * Получает конкретную группу по GUID
	 * @param guid GUID группы
	 * @returns Информация о группе или undefined
	 */
	public getGroupByGuid(guid: string): UserGroupDetailedInfo | undefined {
		return this.groupsByGuidCache.get(guid)
	}

	//#endregion

	//#region Computed свойства

	/**
	 * Маппинг GUID -> группа для быстрого доступа
	 */
	get groupsMap(): Map<string, UserGroupInfo> {
		return computed(() => {
			const groups = this.groupsCache.get()
			const map = new Map<string, UserGroupInfo>()
			groups.forEach((group) => {
				map.set(group.guid, group)
			})
			return map
		}).get()
	}

	/**
	 * Плоский маппинг GUID -> полный путь в дереве
	 */
	get flatTreeMap(): Map<string, string> {
		return computed(() => {
			const map = new Map<string, string>()

			const buildPaths = (nodes: UserGroupTreeInfo[], parentPath: string = '') => {
				nodes.forEach((node) => {
					const fullPath = parentPath ? `${parentPath} > ${node.name}` : node.name
					map.set(node.guid, fullPath)

					if (node.children && node.children.length > 0) {
						buildPaths(node.children, fullPath)
					}
				})
			}

			buildPaths(this.groupsTreeCache.get())
			return map
		}).get()
	}

	/**
	 * Статистика по группам пользователей
	 */
	get statistics() {
		return computed(() => {
			const groups = this.groupsCache.get()
			const tree = this.groupsTreeCache.get()

			const calculateMaxDepth = (nodes: UserGroupTreeInfo[], currentDepth: number = 1): number => {
				if (!nodes || nodes.length === 0) return currentDepth

				let maxDepth = currentDepth
				nodes.forEach((node) => {
					if (node.children && node.children.length > 0) {
						const depth = calculateMaxDepth(node.children, currentDepth + 1)
						maxDepth = Math.max(maxDepth, depth)
					}
				})

				return maxDepth
			}

			return {
				totalGroups: groups.length,
				rootGroups: tree.length,
				groupsWithChildren: tree.filter((node) => node.children && node.children.length > 0).length,
				groupsWithoutChildren: tree.filter((node) => !node.children || node.children.length === 0).length,
				maxDepth: calculateMaxDepth(tree),
			}
		}).get()
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка групп
	 */
	public refreshGroups() {
		this.tryLoadGroups()
	}

	/**
	 * Сигнал обновления дерева групп
	 */
	public refreshTree() {
		this.tryLoadTree()
	}

	/**
	 * Сигнал обновления конкретной группы
	 */
	public refreshGroupByGuid(guid: string) {
		this.tryLoadGroupByGuid(guid)
	}

	/**
	 * Создание новой группы пользователей
	 */
	public async createGroup(data: UserGroupCreateRequest): Promise<string | undefined> {
		const cacheKey = 'creating-group'

		if (this.isLoading(cacheKey)) return undefined
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsCreate(data)

			runInAction(() => {
				logger.info('User group created successfully', {
					component: 'UserGroupsStore',
					method: 'createGroup',
					groupGuid: response.data,
				})

				this.tryLoadGroups()
				this.tryLoadTree()
			})

			return response.data
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create user group'), {
				component: 'UserGroupsStore',
				method: 'createGroup',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Обновление группы пользователей
	 */
	public async updateGroup(guid: string, data: UserGroupUpdateRequest): Promise<void> {
		const cacheKey = `updating-group-${guid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryUserGroupsUpdate(guid, data)

			runInAction(() => {
				logger.info('User group updated successfully', {
					component: 'UserGroupsStore',
					method: 'updateGroup',
					groupGuid: guid,
				})

				this.refreshGroups()
				this.refreshTree()
				this.refreshGroupByGuid(guid)
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update user group'), {
				component: 'UserGroupsStore',
				method: 'updateGroup',
				groupGuid: guid,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Удаление группы пользователей
	 */
	public async deleteGroup(guid: string): Promise<void> {
		const cacheKey = `deleting-group-${guid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryUserGroupsDelete(guid)

			runInAction(() => {
				logger.info('User group deleted successfully', {
					component: 'UserGroupsStore',
					method: 'deleteGroup',
					groupGuid: guid,
				})

				this.refreshGroups()
				this.refreshTree()
				this.groupsByGuidCache.delete(guid) // удаляем кэшированные данные
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete user group'), {
				component: 'UserGroupsStore',
				method: 'deleteGroup',
				groupGuid: guid,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Перемещение группы пользователей
	 */
	public async moveGroup(groupGuid: string, newParentGuid: string | null): Promise<void> {
		const cacheKey = `moving-group-${groupGuid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryUserGroupsMove(groupGuid, { parentGuid: newParentGuid })

			runInAction(() => {
				logger.info('User group moved successfully', {
					component: 'UserGroupsStore',
					method: 'moveGroup',
					groupGuid: groupGuid,
					newParentGuid: newParentGuid,
				})

				this.refreshGroups()
				this.refreshTree()
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to move user group'), {
				component: 'UserGroupsStore',
				method: 'moveGroup',
				groupGuid: groupGuid,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретной группы
	 * @param guid GUID группы
	 */
	public invalidateGroup(guid: string): void {
		runInAction(() => {
			this.groupsByGuidCache.delete(guid)
		})
	}

	//#endregion

	//#region Приватные методы загрузки

	private async tryLoadGroups(): Promise<void> {
		const cacheKey = 'groups'

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetAll()

			if (response.status === 200) {
				runInAction(() => {
					this.groupsCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load user groups'), {
				component: 'UserGroupsStore',
				method: 'tryLoadGroups',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	private async tryLoadTree(): Promise<void> {
		const cacheKey = 'groups-tree'

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetTree()

			if (response.status === 200) {
				runInAction(() => {
					this.groupsTreeCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load user groups tree'), {
				component: 'UserGroupsStore',
				method: 'tryLoadTree',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	private async tryLoadGroupByGuid(guid: string): Promise<void> {
		const cacheKey = `group_${guid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetWithDetails(guid)

			if (response.status === 200) {
				runInAction(() => {
					this.groupsByGuidCache.set(guid, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load user group by guid'), {
				component: 'UserGroupsStore',
				method: 'tryLoadGroupByGuid',
				groupGuid: guid,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion
}
