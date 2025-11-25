import { Api } from '@/generated/Api'
import { UserGroupDetailedInfo, UserGroupInfo, UserGroupTreeInfo } from '@/generated/data-contracts'
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
	private readonly TTL_LIST = 5 * 60 * 1000 // 5 минут для списка
	private readonly TTL_TREE = 5 * 60 * 1000 // 5 минут для дерева
	private readonly TTL_ITEM = 10 * 60 * 1000 // 10 минут для отдельной группы

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
	 * @returns Список групп
	 */
	getGroups(): UserGroupInfo[] {
		const cacheKey = 'groups_list'
		const cached = this._groupsCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadGroups().catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadGroups().catch(console.error)
		}

		return cached ?? []
	}

	/**
	 * Получает дерево групп
	 * @returns Дерево групп
	 */
	getTree(): UserGroupTreeInfo[] {
		const cacheKey = 'groups_tree'
		const cached = this._groupsTreeCache.get()

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
	 * Получает конкретную группу по GUID
	 * @param guid GUID группы
	 * @returns Информация о группе или undefined
	 */
	getGroupByGuid(guid: string): UserGroupDetailedInfo | undefined {
		const cacheKey = `group_${guid}`
		const cached = this._groupsByGuidCache.get(guid)

		if (cached && this.isCacheValid(cacheKey, this.TTL_ITEM)) {
			if (this.shouldRefresh(cacheKey, this.TTL_ITEM)) {
				this.loadGroupByGuid(guid).catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadGroupByGuid(guid).catch(console.error)
		}

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
		this.invalidateCache('groups_list')
		await this.loadGroups()
	}

	/**
	 * Принудительно обновляет дерево групп
	 */
	async refreshTree(): Promise<void> {
		this.invalidateCache('groups_tree')
		await this.loadTree()
	}

	/**
	 * Загружает список групп с сервера
	 */
	private async loadGroups(): Promise<void> {
		const cacheKey = 'groups_list'

		if (this.isLoading(cacheKey)) return

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
			console.error('Failed to load user groups:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает дерево групп с сервера
	 */
	private async loadTree(): Promise<void> {
		const cacheKey = 'groups_tree'

		if (this.isLoading(cacheKey)) return

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
			console.error('Failed to load user groups tree:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретную группу по GUID
	 * @param guid GUID группы
	 */
	private async loadGroupByGuid(guid: string): Promise<void> {
		const cacheKey = `group_${guid}`

		if (this.isLoading(cacheKey)) return

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
			console.error(`Failed to load user group ${guid}:`, error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
