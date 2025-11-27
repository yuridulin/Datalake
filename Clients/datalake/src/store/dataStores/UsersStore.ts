import { Api } from '@/generated/Api'
import { UserInfo, UserWithGroupsInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

/**
 * Store для управления пользователями с кэшированием последнего успешного ответа
 */
export class UsersStore extends BaseCacheStore {
	constructor(private api: Api) {
		super()

		makeObservable(this, {
			getUsers: true,
			getUserByGuid: true,
		})
	}

	/**
	 * Получает список всех пользователей
	 * Возвращает текущий кэш и запускает запрос на сервер, если он еще не идет
	 * @returns Список пользователей из кэша (может быть пустым, пока данные загружаются)
	 */
	public getUsers(): UserInfo[] {
		const reactive = this.usersCache.get()
		this.tryLoadUsers()
		return reactive ?? []
	}

	public refreshUsers() {
		this.tryLoadUsers()
	}

	private usersCacheKey = 'users'
	private usersCache = observable.box<UserInfo[]>([])

	private async tryLoadUsers(): Promise<void> {
		const cacheKey = this.usersCacheKey
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'UsersStore',
				method: 'tryLoadUsers',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUsersGet()

			if (response.status === 200) {
				runInAction(() => {
					this.usersCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to load users'), {
				component: 'UsersStore',
				method: 'tryLoadUsers',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Получает конкретного пользователя по GUID
	 * @param guid GUID пользователя
	 * @returns Информация о пользователе или undefined
	 */
	getUserByGuid(guid: string): UserWithGroupsInfo | undefined {
		const reactive = this.usersByGuidCache.get(guid)
		this.tryLoadUserByGuid(guid)
		return reactive
	}

	private usersByGuidCacheKey = (guid: string) => `user_${guid}`
	private usersByGuidCache = observable.map<string, UserWithGroupsInfo>()

	private async tryLoadUserByGuid(guid: string): Promise<void> {
		const cacheKey = this.usersByGuidCacheKey(guid)
		const isLoading = this.isLoading(cacheKey)

		if (isLoading) {
			logger.debug('Skipping - already loading', {
				component: 'UsersStore',
				method: 'tryLoadUserByGuid',
			})
			return
		}

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUsersGetWithDetails(guid)

			if (response.status === 200) {
				runInAction(() => {
					this.usersByGuidCache.set(guid, response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error(`Failed to load user ${guid}`), {
				component: 'UsersStore',
				method: 'tryLoadUserByGuid',
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретного пользователя
	 * @param guid GUID пользователя
	 */
	invalidateUser(guid: string): void {
		runInAction(() => {
			this.usersByGuidCache.delete(guid)
		})
	}
}
