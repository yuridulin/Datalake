import { Api } from '@/generated/Api'
import { UserGroupDetailedInfo, UserGroupInfo, UserGroupTreeInfo } from '@/generated/data-contracts'
import { CACHE_TTL } from '@/config/cacheConfig'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления группами пользователей с кэшированием
 */
export class UserGroupsStore extends BaseCacheStore {
	// Кэш списка групп
	private _groupsCache = observable.box<UserGroupInfo[] | null>(null)

	// Кэш дерева групп
	private _groupsTreeCache = observable.box<UserGroupTreeInfo[] | null>(null)

	// Кэш отдельных групп по GUID
	private _groupsByGuidCache = observable.map<string, UserGroupDetailedInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = CACHE_TTL.USER_GROUPS.LIST
	private readonly TTL_TREE = CACHE_TTL.USER_GROUPS.TREE
	private readonly TTL_ITEM = CACHE_TTL.USER_GROUPS.ITEM

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getGroups: true,
			getTree: true,
			getGroupByGuid: true,
			isLoadingGroups: true,
			invalidateGroup: true,
			refreshGroups: true,
			refreshTree: true,
		})
	}

	/**
	 * Получает список всех групп
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список групп из кэша (может быть пустым, пока данные загружаются)
	 */
	getGroups(): UserGroupInfo[] {
		const cacheKey = 'groups_list'
		const cached = this._groupsCache.get()

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadGroups('get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'UserGroupsStore',
					method: 'getGroups',
					action: 'loadGroups',
				})
			})
		}

		// Возвращаем текущий кэш (может быть пустым)
		return cached ?? []
	}

	/**
	 * Получает дерево групп
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Дерево групп из кэша (может быть пустым, пока данные загружаются)
	 */
	getTree(): UserGroupTreeInfo[] {
		const cacheKey = 'groups_tree'
		const cached = this._groupsTreeCache.get()

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadTree('get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'UserGroupsStore',
					method: 'getTree',
					action: 'loadTree',
				})
			})
		}

		// Возвращаем текущий кэш (может быть пустым)
		return cached ?? []
	}

	/**
	 * Получает конкретную группу по GUID
	 * @param guid GUID группы
	 * @returns Информация о группе или undefined
	 */
	getGroupByGuid(guid: string): UserGroupDetailedInfo | undefined {
		const cacheKey = `group_${guid}`
		const cached = this._groupsByGuidCache.get(guid)

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadGroupByGuid(guid, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'UserGroupsStore',
					method: 'getGroupByGuid',
					action: 'loadGroupByGuid',
					groupGuid: guid,
				})
			})
		}

		// Возвращаем текущий кэш
		return cached
	}

	/**
	 * Проверяет, идет ли загрузка групп
	 * @returns true, если идет загрузка
	 */
	isLoadingGroups(): boolean {
		return this.isLoading('groups_list')
	}

	/**
	 * Инвалидирует кэш для конкретной группы
	 * @param guid GUID группы
	 */
	invalidateGroup(guid: string): void {
		runInAction(() => {
			this._groupsByGuidCache.delete(guid)
			// Инвалидируем списки, так как они могут содержать эту группу
			this.invalidateCache('groups_list')
			this.invalidateCache('groups_tree')
		})
	}

	/**
	 * Принудительно обновляет список групп
	 */
	async refreshGroups(): Promise<void> {
		// Просто вызываем loadGroups - он сам проверит, идет ли загрузка
		await this.loadGroups('manual-refresh')
	}

	/**
	 * Принудительно обновляет дерево групп
	 */
	async refreshTree(): Promise<void> {
		// Просто вызываем loadTree - он сам проверит, идет ли загрузка
		await this.loadTree('manual-refresh')
	}

	/**
	 * Загружает список групп с сервера
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadGroups(reason: string = 'unknown'): Promise<void> {
		const cacheKey = 'groups_list'

		if (this.isLoading(cacheKey)) {
			logger.debug(`[UserGroupsStore] Skipping loadGroups - already loading`, {
				component: 'UserGroupsStore',
				method: 'loadGroups',
				cacheKey,
				reason,
			})
			return
		}

		logger.info(`[UserGroupsStore] API Request: inventoryUserGroupsGetAll`, {
			component: 'UserGroupsStore',
			method: 'loadGroups',
			cacheKey,
			reason,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetAll()

			if (response.status === 200) {
				runInAction(() => {
					this._groupsCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load user groups'), {
				component: 'UserGroupsStore',
				method: 'loadGroups',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает дерево групп с сервера
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadTree(reason: string = 'unknown'): Promise<void> {
		const cacheKey = 'groups_tree'

		if (this.isLoading(cacheKey)) {
			logger.debug(`[UserGroupsStore] Skipping loadTree - already loading`, {
				component: 'UserGroupsStore',
				method: 'loadTree',
				cacheKey,
				reason,
			})
			return
		}

		logger.info(`[UserGroupsStore] API Request: inventoryUserGroupsGetTree`, {
			component: 'UserGroupsStore',
			method: 'loadTree',
			cacheKey,
			reason,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetTree()

			if (response.status === 200) {
				runInAction(() => {
					this._groupsTreeCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load user groups tree'), {
				component: 'UserGroupsStore',
				method: 'loadTree',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретную группу по GUID
	 * @param guid GUID группы
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadGroupByGuid(guid: string, reason: string = 'unknown'): Promise<void> {
		const cacheKey = `group_${guid}`

		if (this.isLoading(cacheKey)) {
			logger.debug(`[UserGroupsStore] Skipping loadGroupByGuid - already loading`, {
				component: 'UserGroupsStore',
				method: 'loadGroupByGuid',
				cacheKey,
				reason,
				groupGuid: guid,
			})
			return
		}

		logger.info(`[UserGroupsStore] API Request: inventoryUserGroupsGetWithDetails`, {
			component: 'UserGroupsStore',
			method: 'loadGroupByGuid',
			cacheKey,
			reason,
			groupGuid: guid,
		})

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUserGroupsGetWithDetails(guid)

			if (response.status === 200) {
				runInAction(() => {
					this._groupsByGuidCache.set(guid, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load user group ${guid}`), {
				component: 'UserGroupsStore',
				method: 'loadGroupByGuid',
				groupGuid: guid,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
