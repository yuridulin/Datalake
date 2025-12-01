import { logout } from '@/app/router/auth/keycloak/keycloakService'
import routes from '@/app/router/routes'
import { Api } from '@/generated/Api'
import { AccessRuleInfo, AccessType, UserSessionWithAccessInfo, UserType } from '@/generated/data-contracts'
import { NotificationInstance } from 'antd/es/notification/interface'
import { AxiosError, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { makeAutoObservable } from 'mobx'
import { BlocksStore } from './dataStores/BlocksStore'
import { SourcesStore } from './dataStores/SourcesStore'
import { TagsStore } from './dataStores/TagsStore'
import { UserGroupsStore } from './dataStores/UserGroupsStore'
import { UsersStore } from './dataStores/UsersStore'

import { logger } from '@/services/logger'

const debug = false
const log = (...text: unknown[]) => {
	if (!debug) return
	logger.debug(...text)
}

// передача констант с сервера
declare const LOCAL_API: boolean
declare const INSTANCE_NAME: string
declare const VERSION: string

// для клиента, собранного и лежащего в wwwroot, путь к серверу будет на тот же порт, что и у клиента
// для запуска раздельно мы используем порт 8000, который определен в docker compose сервера
let isLocal = false
let instanceName: string | null = null
let version: string = 'DEV'
try {
	isLocal = LOCAL_API
} catch {
	log('LOCAL_API is not defined - set', false)
}
try {
	instanceName = INSTANCE_NAME
} catch {
	log('INSTANCE_NAME is not defined - set', null)
}
try {
	version = VERSION
} catch {
	log('VERSION is not defined - set', 'DEV')
}

// константы заголовков и ключей localStorage
const sessionTokenHeader = 'X-Session-Token'
const themeKey = 'd-theme'

const emptyRule: AccessRuleInfo = { ruleId: 0, access: AccessType.None }

export class AppStore {
	//#region Инициализация

	// объект работы с сервером
	api: Api

	// состояние сервера
	isLoading = true
	isConnected = false
	isAuthenticated = false

	// Текущие поля
	isDark: boolean = false
	instanceName: string
	version: string

	// Stores для кэширования данных
	tagsStore: TagsStore
	blocksStore: BlocksStore
	sourcesStore: SourcesStore
	usersStore: UsersStore
	userGroupsStore: UserGroupsStore

	constructor() {
		makeAutoObservable(this)
		log('loading...')
		this.initTheme()
		this.api = this.createApiClient()
		this.instanceName = 'Datalake' + (instanceName ? ' | ' + instanceName : '')
		this.version = version

		// Инициализируем stores
		this.tagsStore = new TagsStore(this.api)
		this.blocksStore = new BlocksStore(this.api)
		this.sourcesStore = new SourcesStore(this.api)
		this.usersStore = new UsersStore(this.api)
		this.userGroupsStore = new UserGroupsStore(this.api)

		if (window.location.pathname !== routes.auth.keycloak) {
			this.refreshAuthData()
		}
	}

	private createApiClient() {
		const api = new Api({
			baseURL: isLocal ? window.location.origin + '/' : 'http://localhost:7600/',
			validateStatus(status) {
				return status >= 200 && status < 300
			},
		})

		api.instance.interceptors.request.use(
			(config) => {
				log('SEND TOKEN ', this.sessionToken, 'IsAuthenticated: ', this.isAuthenticated)
				config.headers[sessionTokenHeader] = this.sessionToken
				return config
			},
			(err) => Promise.reject(err),
		)

		api.instance.interceptors.response.use(
			(response: AxiosResponse) => {
				this.setConnectionStatus(true)

				// запросы, которые не обрабатываем
				if (response.config.method === 'OPTIONS') {
					return response
				}
				// переход на логин, если нет доступа
				else if (response.status === 401 || response.status === 400) {
					this.setAuthenticated(false)
				}
				// нормальное развитие событий
				else {
					if (response.status === 204 && this.notify) {
						this.notify.success({ placement: 'bottomLeft', message: `Успешно` })
					}
					// сообщения после выполнения действий
					else if (response.data) {
						if (response.data.done && this.notify) {
							log(this.notify)
							this.notify.info({ placement: 'bottomLeft', message: response.data.done })
						}
						if (response.data.error && this.notify) {
							this.notify.error({ placement: 'bottomLeft', message: response.data.error })
						}
					}
				}

				return response
			},
			async (error: AxiosError) => {
				this.setConnectionStatus(error.request?.status !== 0 /*  || error.code === 'ERR_NETWORK' */)

				const statusCode = error.response?.status
				const config = error.config as InternalAxiosRequestConfig

				if (statusCode === 401 || statusCode === 400) {
					this.setAuthenticated(false)
					if (!config?.url?.endsWith('identify') && this.notify)
						this.notify.error({ placement: 'bottomLeft', message: String(error.response?.data) })
					return Promise.reject(error)
				}

				// сообщения после выполнения действий
				if (error.request?.status === 500) {
					let message = (error.response?.data as { error: string })?.error ?? (error.request?.responseText as string)
					if (message.indexOf('\n\n') > -1) message = message.substring(0, message.indexOf('\n\n'))
					this.notify?.error({ placement: 'bottomLeft', message: message })
					return Promise.reject(error)
				}

				// сообщения о транспортной ошибке
				else if (this.isConnected) {
					this.notify?.error({ placement: 'bottomLeft', message: error.message })
					return Promise.reject(error)
				}

				return Promise.reject(error)
			},
		)

		return api
	}

	private initTheme = () => {
		const storedTheme = localStorage.getItem(themeKey)
		const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
		this.isDark = storedTheme ? storedTheme === 'dark' : prefersDark
		localStorage.setItem(themeKey, this.isDark ? 'dark' : 'light')
	}

	//#endregion Инициализация

	//#region Уведомления

	notify: NotificationInstance | null = null

	setNotify = (notification: NotificationInstance) => {
		this.notify = notification
	}

	//#endregion Уведомления

	//#region Изменения состояния

	switchTheme = () => {
		log('SET store.isDark =', !this.isDark)
		this.isDark = !this.isDark
		localStorage.setItem(themeKey, this.isDark ? 'dark' : 'light')
	}

	// Методы для соединения
	setConnectionStatus = (status: boolean) => {
		log('SET store.isConnected =', status)
		this.isConnected = status
	}

	setAuthenticated = (flag: boolean) => {
		log('SET store.isAuthenticated =', flag)
		this.isAuthenticated = flag
	}

	doneLoading = () => {
		if (this.isLoading) log('loading is complete')
		this.isLoading = false
	}

	//#endregion Изменения состояния

	//#region Методы аутентификации

	// сохраняемые в LS настройки
	sessionToken: string = localStorage.getItem(sessionTokenHeader) || ''

	// настройки с бэкенда
	userGuid: string | null = null
	type: UserType | null = null
	rootRule: AccessRuleInfo = emptyRule
	groups: Record<string, AccessRuleInfo> = {}
	sources: Record<number, AccessRuleInfo> = {}
	blocks: Record<number, AccessRuleInfo> = {}
	tags: Record<string, AccessRuleInfo> = {}

	public setAuthData = (session: UserSessionWithAccessInfo) => {
		const access = session.access

		log('SET store.sessionToken =', session.token)
		this.sessionToken = session.token
		localStorage.setItem(sessionTokenHeader, session.token)

		this.userGuid = session.userGuid
		this.type = session.type
		this.rootRule = access.rootRule

		this.sources = access.sources
		this.blocks = access.blocks
		this.tags = access.tags
		this.groups = access.groups

		this.setAuthenticated(true)
	}

	public refreshAuthData = () => {
		return this.api
			.authIdentify()
			.then((res) => {
				if (res.status === 200) {
					this.setAuthData(res.data)
				}
			})
			.catch(() => this.setAuthenticated(false))
			.finally(() => this.doneLoading())
	}

	public clearAuthData = () => {
		log('REMOVE store.sessionToken')
		this.sessionToken = ''
		localStorage.removeItem(sessionTokenHeader)

		this.userGuid = null
		this.type = null
		this.rootRule = emptyRule

		this.sources = {}
		this.blocks = {}
		this.tags = {}
		this.groups = {}

		this.setAuthenticated(false)
	}

	public loginLocal = (login: string, password: string) => {
		this.api
			.authAuthenticateLocal({
				login: login,
				password: password,
			})
			.then((res) => {
				if (res.status === 200) {
					this.setAuthData(res.data)
				}
			})
			.catch(() => this.setAuthenticated(false))
	}

	public loginKeycloak = (guid: string, email: string, name: string) => {
		this.api
			.authAuthenticateEnergoIdUser({
				energoIdGuid: guid,
				email: email,
				fullName: name,
			})
			.then((res) => {
				if (res.status === 200) {
					this.setAuthData(res.data)
				}
			})
			.catch(() => this.setAuthenticated(false))
	}

	public logout = () => {
		this.api.authLogout().then(() => {
			if (this.type === UserType.EnergoId) logout()
			this.clearAuthData()
		})
	}

	//#endregion

	//#region Проверки прав

	hasGlobalAccess(minimal: AccessType): boolean {
		return this.getGlobalAccess() >= minimal
	}

	hasAccessToSource(minimal: AccessType, id: number): boolean {
		return this.getAccessToSource(id) >= minimal
	}

	hasAccessToBlock(minimal: AccessType, id: number): boolean {
		return this.getAccessToBlock(id) >= minimal
	}

	hasAccessToTag(minimal: AccessType, id: number): boolean {
		return this.getAccessToTag(id) >= minimal
	}

	hasAccessToGroup(minimal: AccessType, guid: string): boolean {
		return this.getAccessToGroup(guid) >= minimal
	}

	//#endregion Проверки прав

	//#region Получение прав

	getGlobalAccess(): AccessType {
		return this.rootRule.access
	}

	getAccessToSource(id: number): AccessType {
		return this.sources[id]?.ruleId ?? this.rootRule.ruleId
	}

	getAccessToBlock(id: number) {
		return this.blocks[id]?.ruleId ?? this.rootRule.ruleId
	}

	getAccessToTag(id: number) {
		return this.tags[id]?.ruleId ?? this.rootRule.ruleId
	}

	getAccessToGroup(guid: string) {
		return this.groups[guid]?.ruleId ?? this.rootRule.ruleId
	}

	//#endregion Проверки прав
}

export const appStore = new AppStore()
