import { Api } from '@/generated/Api'
import { UserInfo, UserWithGroupsInfo } from '@/generated/data-contracts'
import { CACHE_TTL } from '@/config/cacheConfig'
import { logger } from '@/services/logger'
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
	private readonly TTL_LIST = CACHE_TTL.USERS.LIST
	private readonly TTL_ITEM = CACHE_TTL.USERS.ITEM

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
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список пользователей из кэша (может быть пустым, пока данные загружаются)
	 */
	getUsers(): UserInfo[] {
		const cacheKey = 'users_list'
		const cached = this._usersCache.get()

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadUsers('get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'UsersStore',
					method: 'getUsers',
					action: 'loadUsers',
				})
			})
		}

		// Возвращаем текущий кэш (может быть пустым)
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

		// Если запрос не идет, запускаем его
		if (!this.isLoading(cacheKey)) {
			this.loadUserByGuid(guid, 'get-request').catch((error) => {
				logger.error(error instanceof Error ? error : new Error(String(error)), {
					component: 'UsersStore',
					method: 'getUserByGuid',
					action: 'loadUserByGuid',
					userGuid: guid,
				})
			})
		}

		// Возвращаем текущий кэш
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
	 * Проверяет, есть ли данные в кэше для списка пользователей
	 * @returns true, если данные были загружены хотя бы раз
	 */
	hasUsersCache(): boolean {
		return this.hasCache('users_list')
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
		// Просто вызываем loadUsers - он сам проверит, идет ли загрузка
		await this.loadUsers('manual-refresh')
	}

	/**
	 * Загружает список пользователей с сервера
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadUsers(reason: string = 'unknown'): Promise<void> {
		const cacheKey = 'users_list'

		if (this.isLoading(cacheKey)) {
			logger.debug(`[UsersStore] Skipping loadUsers - already loading`, {
				component: 'UsersStore',
				method: 'loadUsers',
				cacheKey,
				reason,
			})
			return
		}

		logger.info(`[UsersStore] API Request: inventoryUsersGet`, {
			component: 'UsersStore',
			method: 'loadUsers',
			cacheKey,
			reason,
		})

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
			logger.error(error instanceof Error ? error : new Error('Failed to load users'), {
				component: 'UsersStore',
				method: 'loadUsers',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает конкретного пользователя по GUID
	 * @param guid GUID пользователя
	 * @param reason Причина вызова запроса (для логирования)
	 */
	private async loadUserByGuid(guid: string, reason: string = 'unknown'): Promise<void> {
		const cacheKey = `user_${guid}`

		if (this.isLoading(cacheKey)) {
			logger.debug(`[UsersStore] Skipping loadUserByGuid - already loading`, {
				component: 'UsersStore',
				method: 'loadUserByGuid',
				cacheKey,
				reason,
				userGuid: guid,
			})
			return
		}

		logger.info(`[UsersStore] API Request: inventoryUsersGetWithDetails`, {
			component: 'UsersStore',
			method: 'loadUserByGuid',
			cacheKey,
			reason,
			userGuid: guid,
		})

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
			logger.error(error instanceof Error ? error : new Error(`Failed to load user ${guid}`), {
				component: 'UsersStore',
				method: 'loadUserByGuid',
				userGuid: guid,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
