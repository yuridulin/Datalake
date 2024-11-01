import { makeAutoObservable } from 'mobx'
import { AccessType } from './swagger/data-contracts'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'

class User {
	constructor() {
		this.name = localStorage.getItem(nameHeader) || ''
		this.token = localStorage.getItem(tokenHeader) || ''
		this.globalAccess = AccessType[
			(localStorage.getItem(accessHeader) || '') as unknown as AccessType
		] as unknown as AccessType
		makeAutoObservable(this)
	}

	name: string = ''
	token: string = ''
	globalAccess: AccessType = AccessType.NoAccess

	isAuth() {
		return this.token !== ''
	}

	setName(name: string) {
		this.name = name
		localStorage.setItem(nameHeader, name)
	}

	setToken(token: string) {
		this.token = token
		localStorage.setItem(tokenHeader, token)
	}

	setAccess(access: AccessType) {
		this.globalAccess = AccessType[access] as unknown as AccessType
		localStorage.setItem(accessHeader, access.toString())
	}

	logout() {
		this.setName('')
		this.setToken('')
		this.setAccess(AccessType.NotSet)
	}
}

const user = new User()

export { accessHeader as globalAccessHeader, nameHeader, tokenHeader, user }
