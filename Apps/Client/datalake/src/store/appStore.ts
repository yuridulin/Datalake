import { logout } from '@/app/router/auth/keycloak/keycloakService'
import routes from '@/app/router/routes'
import { Api } from '@/generated/Api'
import { AccessRuleInfo, AccessType, UserAuthInfo, UserSessionInfo, UserType } from '@/generated/data-contracts'
import { NotificationInstance } from 'antd/es/notification/interface'
import { AxiosError, AxiosResponse } from 'axios'
import dayjs from 'dayjs'
import 'dayjs/locale/ru'
import TimeAgo from 'javascript-time-ago'
import ru from 'javascript-time-ago/locale/ru'
import { makeAutoObservable } from 'mobx'
import hasAccess from '../functions/hasAccess'

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
	console.log('LOCAL_API is not defined - set', false)
}
try {
	instanceName = INSTANCE_NAME
} catch {
	console.log('INSTANCE_NAME is not defined - set', null)
}
try {
	version = VERSION
} catch {
	console.log('VERSION is not defined - set', 'DEV')
}

// константы заголовков и ключей localStorage
const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'
const identityHeader = 'd-identity'
const themeKey = 'd-theme'

// настройка времени
dayjs.locale('ru')
TimeAgo.addLocale(ru)
const timeAgo = new TimeAgo('ru-RU')
export { timeAgo }

export class AppStore implements UserAuthInfo {
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

	constructor() {
		makeAutoObservable(this)
		console.log('loading...')
		this.initTheme()
		this.api = this.createApiClient()
		this.instanceName = 'Datalake' + (instanceName ? ' | ' + instanceName : '')
		this.version = version
		if (window.location.pathname !== routes.auth.keycloak) {
			this.refreshAuthData()
		}
	}

	private createApiClient() {
		const api = new Api({
			baseURL: isLocal ? window.location.origin + '/' : 'http://localhost:7630/',
			validateStatus(status) {
				return status >= 200 && status < 300
			},
		})

		api.instance.interceptors.request.use(
			(config) => {
				console.log('SEND TOKEN ', this.token, 'IsAuthenticated: ', this.isAuthenticated)
				config.headers[tokenHeader] = this.token
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
				else if (response.status === 403 || response.status === 400) {
					this.setAuthenticated(false)
				}
				// нормальное развитие событий
				else {
					// сообщения после выполнения действий
					if (response.data) {
						if (response.data.done && this.notify) {
							console.log(this.notify)
							this.notify.info({ placement: 'bottomLeft', message: response.data.done })
						}
						if (response.data.error && this.notify) {
							this.notify.error({ placement: 'bottomLeft', message: response.data.error })
						}
					}
				}

				return response
			},
			(error: AxiosError) => {
				this.setConnectionStatus(error.request?.status !== 0 /*  || error.code === 'ERR_NETWORK' */)

				if (error.response?.status === 403 || error.response?.status === 400) {
					this.setAuthenticated(false)
					if (!error.config?.url?.endsWith('identify') && this.notify)
						this.notify.error({ placement: 'bottomLeft', message: String(error.response?.data) })
					return
				}

				// сообщения после выполнения действий
				if (error.request?.status === 500) {
					let message = (error.response?.data as { error: string })?.error ?? (error.request?.responseText as string)
					if (message.indexOf('\n\n') > -1) message = message.substring(0, message.indexOf('\n\n'))
					return this.notify?.error({ placement: 'bottomLeft', message: message })
				}

				// сообщения о транспортной ошибке
				else if (this.isConnected) {
					return this.notify?.error({ placement: 'bottomLeft', message: error.message })
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
		console.log('SET store.isDark =', !this.isDark)
		this.isDark = !this.isDark
		localStorage.setItem(themeKey, this.isDark ? 'dark' : 'light')
	}

	// Методы для соединения
	setConnectionStatus = (status: boolean) => {
		console.log('SET store.isConnected =', status)
		this.isConnected = status
	}

	setAuthenticated = (flag: boolean) => {
		console.log('SET store.isAuthenticated =', flag)
		this.isAuthenticated = flag
	}

	doneLoading = () => {
		if (this.isLoading) console.log('loading is complete')
		this.isLoading = false
	}

	//#endregion Изменения состояния

	//#region Методы аутентификации

	// сохраняемые в LS настройки
	fullName: string = localStorage.getItem(nameHeader) || ''
	token: string = localStorage.getItem(tokenHeader) || ''
	globalAccessType: AccessType = Number(localStorage.getItem(accessHeader) || AccessType.NotSet) as AccessType

	// настройки с бэкенда
	guid: string = ''
	type: UserType | null = null
	rootRule!: AccessRuleInfo
	underlyingUser?: UserAuthInfo | null | undefined
	energoId?: string | null | undefined
	accessRule: AccessRuleInfo = { ruleId: 0, access: this.globalAccessType }
	groups: Record<string, AccessRuleInfo> = {}
	sources: Record<number, AccessRuleInfo> = {}
	blocks: Record<number, AccessRuleInfo> = {}
	tags: Record<string, AccessRuleInfo> = {}

	public setAuthData = (session: UserSessionInfo) => {
		const data = session.authInfo

		console.log('SET store.authToken =', session.token)
		console.log('SET store.authLogin =', data.fullName)
		console.log('SET store.globalAccessType =', data.rootRule.access)

		this.type = session.type
		this.token = session.token
		this.fullName = data.fullName
		this.globalAccessType = data.rootRule.access
		this.sources = data.sources
		this.blocks = data.blocks
		this.tags = data.tags
		this.groups = data.groups
		this.rootRule = data.rootRule

		localStorage.setItem(tokenHeader, session.token)
		localStorage.setItem(nameHeader, data.fullName)
		localStorage.setItem(accessHeader, String(data.rootRule.access))
		localStorage.setItem(identityHeader, JSON.stringify(data))

		this.setAuthenticated(true)
	}

	public refreshAuthData = () => {
		this.api
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
		console.log('SET store.authToken =', '')
		console.log('SET store.authLogin =', '')
		console.log('SET store.globalAccessType =', AccessType.NotSet)
		this.token = ''
		this.fullName = ''
		this.globalAccessType = AccessType.NotSet
		localStorage.removeItem(tokenHeader)
		localStorage.removeItem(nameHeader)
		localStorage.removeItem(accessHeader)
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
		this.api.authLogout({ token: this.token }).then(() => {
			if (this.type === UserType.EnergoId) logout()
			this.clearAuthData()
		})
	}

	//#endregion

	//#region Проверки прав

	hasGlobalAccess(minimal: AccessType) {
		return hasAccess(this.globalAccessType, minimal)
	}

	hasAccessToSource(minimal: AccessType, id: number) {
		const rule = this.sources[id] ?? this.rootRule
		return hasAccess(rule?.access ?? this.globalAccessType, minimal)
	}

	hasAccessToBlock(minimal: AccessType, id: number) {
		const rule = this.blocks[id] ?? this.rootRule
		return hasAccess(rule?.access ?? this.globalAccessType, minimal)
	}

	hasAccessToTag(minimal: AccessType, id: number) {
		const rule = this.tags[id] ?? this.rootRule
		return hasAccess(rule?.access ?? this.globalAccessType, minimal)
	}

	hasAccessToGroup(minimal: AccessType, guid: string) {
		const rule = this.groups[guid] ?? this.rootRule
		return hasAccess(rule?.access ?? this.globalAccessType, minimal)
	}

	//#endregion Проверки прав
}

export const appStore = new AppStore()
