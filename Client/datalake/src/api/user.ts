import { makeAutoObservable } from 'mobx'
import { AccessRule, AccessType, UserAuthInfo } from './swagger/data-contracts'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'

class User implements UserAuthInfo {
	constructor() {
		this.fullName = localStorage.getItem(nameHeader) || ''
		this.token = localStorage.getItem(tokenHeader) || ''
		this.globalAccessType = AccessType[
			(localStorage.getItem(accessHeader) || '') as unknown as AccessType
		] as unknown as AccessType

		makeAutoObservable(this)
	}

	guid: string = ''
	fullName: string
	globalAccessType: AccessType
	groups?: Record<string, AccessRule> | undefined = {}
	sources?: Record<string, AccessRule> | undefined = {}
	blocks?: Record<string, AccessRule> | undefined = {}
	tags?: Record<string, AccessRule> | undefined = {}
	token: string = ''

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
}

const user = new User()

export { accessHeader as globalAccessHeader, nameHeader, tokenHeader, user }
