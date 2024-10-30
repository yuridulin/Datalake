import { makeAutoObservable } from 'mobx'
import { AccessType } from './swagger/data-contracts'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const globalAccessHeader = 'd-access-type'

class User {
	constructor() {
		this.name = localStorage.getItem(nameHeader) || ''
		makeAutoObservable(this)
	}

	name: string = ''
	globalAccess: AccessType = AccessType.NoAccess

	setAccess(access: AccessType) {
		this.globalAccess = AccessType[access] as unknown as AccessType
	}

	setName(name: string) {
		this.name = name
		localStorage.setItem(nameHeader, name)
	}
}

const user = new User()

const isAuth = () => localStorage.getItem(tokenHeader) !== ''

const getToken = () => localStorage.getItem(tokenHeader)
const setToken = (token: string) => localStorage.setItem(tokenHeader, token)
const freeToken = () => localStorage.setItem(tokenHeader, '')

export {
	freeToken,
	getToken,
	globalAccessHeader,
	isAuth,
	nameHeader,
	setToken,
	tokenHeader,
	user,
}
