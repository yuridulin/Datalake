// UsersStore.ts
import { Api } from '@/generated/Api'
import { UserCreateRequest, UserInfo, UserUpdateRequest, UserWithGroupsInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from '../abstractions/BaseCacheStore'

export class UsersStore extends BaseCacheStore {
	// Observable данные
	private usersCache = observable.box<UserInfo[]>([])
	private usersByGuidCache = observable.map<string, UserWithGroupsInfo>()

	constructor(private api: Api) {
		super()
		makeObservable(this, {})
	}

	//#region Методы получения

	/**
	 * Получает список всех пользователей
	 * @returns Список пользователей из кэша (может быть пустым, пока данные загружаются)
	 */
	public getUsers(): UserInfo[] {
		return this.usersCache.get()
	}

	/**
	 * Получает конкретного пользователя по GUID
	 * @param guid GUID пользователя
	 * @returns Информация о пользователе или undefined
	 */
	public getUserByGuid(guid: string): UserWithGroupsInfo | undefined {
		return this.usersByGuidCache.get(guid)
	}

	//#endregion

	//#region Публичные методы для компонентов

	/**
	 * Сигнал обновления списка пользователей
	 */
	public refreshUsers() {
		this.tryLoadUsers()
	}

	/**
	 * Сигнал обновления конкретного пользователя
	 */
	public refreshUserByGuid(guid: string) {
		this.tryLoadUserByGuid(guid)
	}

	/**
	 * Создание нового пользователя
	 */
	public async createUser(data: UserCreateRequest): Promise<string | undefined> {
		const cacheKey = 'creating-user'

		if (this.isLoading(cacheKey)) return undefined
		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventoryUsersCreate(data)

			runInAction(() => {
				logger.info('User created successfully', {
					component: 'UsersStore',
					method: 'createUser',
					userGuid: response.data,
				})

				this.tryLoadUsers()
			})

			return response.data
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create user'), {
				component: 'UsersStore',
				method: 'createUser',
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Обновление пользователя
	 */
	public async updateUser(guid: string, data: UserUpdateRequest): Promise<void> {
		const cacheKey = `updating-user-${guid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryUsersUpdate(guid, data)

			runInAction(() => {
				logger.info('User updated successfully', {
					component: 'UsersStore',
					method: 'updateUser',
					userGuid: guid,
				})

				this.refreshUsers()
				this.refreshUserByGuid(guid)
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update user'), {
				component: 'UsersStore',
				method: 'updateUser',
				userGuid: guid,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Удаление пользователя
	 */
	public async deleteUser(guid: string): Promise<void> {
		const cacheKey = `deleting-user-${guid}`

		if (this.isLoading(cacheKey)) return
		this.setLoading(cacheKey, true)

		try {
			await this.api.inventoryUsersDelete(guid)

			runInAction(() => {
				logger.info('User deleted successfully', {
					component: 'UsersStore',
					method: 'deleteUser',
					userGuid: guid,
				})

				this.refreshUsers()
				this.usersByGuidCache.delete(guid) // удаляем кэшированные данные
			})
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete user'), {
				component: 'UsersStore',
				method: 'deleteUser',
				userGuid: guid,
			})
			throw error
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Инвалидирует кэш для конкретного пользователя
	 * @param guid GUID пользователя
	 */
	public invalidateUser(guid: string): void {
		runInAction(() => {
			this.usersByGuidCache.delete(guid)
		})
	}

	/**
	 * Проверяет, идет ли загрузка списка пользователей
	 */
	public isLoadingUsers(): boolean {
		return this.isLoading('users')
	}

	/**
	 * Проверяет, идет ли загрузка конкретного пользователя
	 * @param guid GUID пользователя
	 */
	public isLoadingUser(guid: string): boolean {
		return this.isLoading(`user_${guid}`)
	}

	//#endregion

	//#region Приватные методы загрузки

	private async tryLoadUsers(): Promise<void> {
		const cacheKey = 'users'

		if (this.isLoading(cacheKey)) return
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

	private async tryLoadUserByGuid(guid: string): Promise<void> {
		const cacheKey = `user_${guid}`

		if (this.isLoading(cacheKey)) return
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
			logger.error(error instanceof Error ? error : new Error('Failed to load user by guid'), {
				component: 'UsersStore',
				method: 'tryLoadUserByGuid',
				userGuid: guid,
			})
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	//#endregion
}
