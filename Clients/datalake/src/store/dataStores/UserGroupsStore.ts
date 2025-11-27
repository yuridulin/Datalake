import { Api } from '@/generated/Api'
import { UserGroupDetailedInfo, UserGroupInfo, UserGroupTreeInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления группами пользователей с кэшированием последнего успешного ответа
 */
export class UserGroupsStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getGroups: true,
			getTree: true,
			getGroupByGuid: true,
		})
	}

	/**
	 * Получает список всех групп
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список групп из кэша (может быть пустым, пока данные загружаются)
	 */
	public getGroups(): UserGroupInfo[] {
		const reactive = this.groupsCache.get()
		this.tryLoadGroups()
		return reactive ?? []
	}

	public refreshGroups() {
		this.tryLoadGroups()
	}

	private groupsCacheKey = 'groups'
	private groupsCache = observable.box<UserGroupInfo[]>([])

	private async tryLoadGroups(): Promise<void> {
		const cacheKey = this.groupsCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'UserGroupsStore',
				method: 'tryLoadGroups',
			})
			return
		}

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

	/**
	 * Получает дерево групп
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Дерево групп из кэша (может быть пустым, пока данные загружаются)
	 */
	public getTree(): UserGroupTreeInfo[] {
		const reactive = this.groupsTreeCache.get()
		this.tryLoadTree()
		return reactive ?? []
	}

	public refreshTree() {
		this.tryLoadTree()
	}

	private groupsTreeCacheKey = 'groups-tree'
	private groupsTreeCache = observable.box<UserGroupTreeInfo[]>([])

	private async tryLoadTree(): Promise<void> {
		const cacheKey = this.groupsTreeCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'UserGroupsStore',
				method: 'tryLoadTree',
			})
			return
		}

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

	/**
	 * Получает конкретную группу по GUID
	 * @param guid GUID группы
	 * @returns Информация о группе или undefined
	 */
	getGroupByGuid(guid: string): UserGroupDetailedInfo | undefined {
		const reactive = this.groupsByGuidCache.get(guid)
		this.tryLoadGroupByGuid(guid)
		return reactive
	}

	private groupsByGuidCacheKey = (guid: string) => `group_${guid}`
	private groupsByGuidCache = observable.map<string, UserGroupDetailedInfo>()

	private async tryLoadGroupByGuid(guid: string): Promise<void> {
		const cacheKey = this.groupsByGuidCacheKey(guid)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'UserGroupsStore',
				method: 'tryLoadGroupByGuid',
			})
			return
		}

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
			logger.error(error instanceof Error ? error : new Error(`Failed to load user group ${guid}`), {
				component: 'UserGroupsStore',
				method: 'tryLoadGroupByGuid',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретной группы
	 * @param guid GUID группы
	 */
	invalidateGroup(guid: string): void {
		runInAction(() => {
			this.groupsByGuidCache.delete(guid)
		})
	}
}
