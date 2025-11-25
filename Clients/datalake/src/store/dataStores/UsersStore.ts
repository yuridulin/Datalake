import { Api } from '@/generated/Api'
import { UserInfo, UserWithGroupsInfo } from '@/generated/data-contracts'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления пользователями с кэшированием
 */
export class UsersStore extends BaseCacheStore {
	// Кэш списка пользователей
	private _usersCache = observable.box<UserInfo[] | null>(null)

	// Кэш отдельных пользователей по GUID
	private _usersByGuidCache = observable.map<string, UserWithGroupsInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = 5 * 60 * 1000 // 5 минут для списка
	private readonly TTL_ITEM = 10 * 60 * 1000 // 10 минут для отдельного пользователя

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getUsers: true,
			getUserByGuid: true,
			isLoadingUsers: true,
			invalidateUser: true,
			refreshUsers: true,
		})
	}

	/**
	 * Получает список всех пользователей
	 * @returns Список пользователей
	 */
	getUsers(): UserInfo[] {
		const cacheKey = 'users_list'
		const cached = this._usersCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadUsers().catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadUsers().catch(console.error)
		}

		return cached ?? []
	}

	/**
	 * Получает конкретного пользователя по GUID
	 * @param guid GUID пользователя
	 * @returns Информация о пользователе или undefined
	 */
	getUserByGuid(guid: string): UserWithGroupsInfo | undefined {
		const cacheKey = `user_${guid}`
		const cached = this._usersByGuidCache.get(guid)

		if (cached && this.isCacheValid(cacheKey, this.TTL_ITEM)) {
			if (this.shouldRefresh(cacheKey, this.TTL_ITEM)) {
				this.loadUserByGuid(guid).catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadUserByGuid(guid).catch(console.error)
		}

		return cached
	}

	/**
	 * Проверяет, идет ли загрузка пользователей
	 * @returns true, если идет загрузка
	 */
	isLoadingUsers(): boolean {
		return this.isLoading('users_list')
	}

	/**
	 * Инвалидирует кэш для конкретного пользователя
	 * @param guid GUID пользователя
	 */
	invalidateUser(guid: string): void {
		runInAction(() => {
			this._usersByGuidCache.delete(guid)
			// Инвалидируем список, так как он может содержать этого пользователя
			this.invalidateCache('users_list')
		})
	}

	/**
	 * Принудительно обновляет список пользователей
	 */
	async refreshUsers(): Promise<void> {
		this.invalidateCache('users_list')
		await this.loadUsers()
	}

	/**
	 * Загружает список пользователей с сервера
	 */
	private async loadUsers(): Promise<void> {
		const cacheKey = 'users_list'

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUsersGet()

			if (response.status === 200) {
				runInAction(() => {
					this._usersCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error('Failed to load users:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретного пользователя по GUID
	 * @param guid GUID пользователя
	 */
	private async loadUserByGuid(guid: string): Promise<void> {
		const cacheKey = `user_${guid}`

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUsersGetWithDetails(guid)

			if (response.status === 200) {
				runInAction(() => {
					// UserWithGroupsInfo может отличаться от UserInfo
					// Сохраняем как есть, так как это детализированная информация
					this._usersByGuidCache.set(guid, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error(`Failed to load user ${guid}:`, error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
