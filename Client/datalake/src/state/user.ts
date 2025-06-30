import { makeAutoObservable } from 'mobx'
import { AccessRuleInfo, AccessType, UserAuthInfo } from '../api/swagger/data-contracts'
import hasAccess from '../functions/hasAccess'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'
const themeKey = 'd-theme'

class User implements UserAuthInfo {
	constructor() {
		this.fullName = localStorage.getItem(nameHeader) || ''
		this.token = localStorage.getItem(tokenHeader) || ''
		this.globalAccessType = AccessType[
			(localStorage.getItem(accessHeader) || '') as unknown as AccessType
		] as unknown as AccessType
		this.accessRule = { ruleId: 0, access: this.globalAccessType }

		//debugger
		const storedTheme = localStorage.getItem(themeKey)
		if (storedTheme == null) {
			this.theme = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
			localStorage.setItem(themeKey, this.theme)
		} else {
			this.theme = storedTheme == 'dark' ? 'dark' : 'light'
		}

		makeAutoObservable(this)
	}
	rootRule!: AccessRuleInfo
	underlyingUser?: UserAuthInfo | null | undefined
	energoId?: string | null | undefined

	guid: string = ''
	fullName: string
	globalAccessType: AccessType
	accessRule: AccessRuleInfo
	groups: Record<string, AccessRuleInfo> = {}
	sources: Record<number, AccessRuleInfo> = {}
	blocks: Record<number, AccessRuleInfo> = {}
	tags: Record<string, AccessRuleInfo> = {}
	token: string = ''
	theme: 'dark' | 'light'

	isAuth() {
		return this.token !== ''
	}

	setName(name: string) {
		this.fullName = name
		localStorage.setItem(nameHeader, name)
	}

	setToken(token: string) {
		this.token = token
		localStorage.setItem(tokenHeader, token)
	}

	setAccess(access: AccessType) {
		this.globalAccessType = AccessType[access] as unknown as AccessType
		localStorage.setItem(accessHeader, access.toString())
	}

	isDark() {
		return this.theme === 'dark'
	}

	setTheme(theme: 'dark' | 'light') {
		//debugger
		this.theme = theme
		localStorage.setItem(themeKey, theme)
	}

	identify(authInfo: UserAuthInfo) {
		this.sources = authInfo.sources
		this.blocks = authInfo.blocks
		this.tags = authInfo.tags
		this.groups = authInfo.groups

		console.log('user:', JSON.parse(JSON.stringify(this)))
	}

	logout() {
		this.setName('')
		this.setToken('')
		this.setAccess(AccessType.NotSet)
	}

	hasGlobalAccess(minimal: AccessType) {
		return hasAccess(this.globalAccessType, minimal)
	}

	hasAccessToSource(minimal: AccessType, id: number) {
		const rule = this.sources[id] as AccessRuleInfo
		return hasAccess(rule?.access ?? AccessType.NotSet, minimal)
	}

	hasAccessToBlock(minimal: AccessType, id: number) {
		const rule = this.blocks[id]
		return hasAccess(rule?.access ?? AccessType.NotSet, minimal)
	}

	hasAccessToTag(minimal: AccessType, id: number) {
		const rule = this.tags[id]
		return hasAccess(rule?.access ?? AccessType.NotSet, minimal)
	}

	hasAccessToGroup(minimal: AccessType, guid: string) {
		const rule = this.groups[guid]
		return hasAccess(rule?.access ?? AccessType.NotSet, minimal)
	}
}

const user = new User()

export { accessHeader as globalAccessHeader, nameHeader, tokenHeader, user }
