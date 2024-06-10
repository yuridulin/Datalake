import { makeAutoObservable } from 'mobx'
import api from '../api/swagger-api'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'

class Auth {
	token = ''
	fullName = ''
	isAuthenticated = false

	constructor() {
		makeAutoObservable(this)
		this.token = localStorage.getItem(tokenHeader) ?? ''
		this.fullName = localStorage.getItem(nameHeader) ?? ''
		this.isAuthenticated = !!this.token
		this.identify()
	}

	getSessionToken() {
		return this.token
	}

	setSessionToken(newToken: string) {
		if (newToken !== this.token) {
			this.identify()
		}
		this.token = newToken
		localStorage.setItem(tokenHeader, newToken)
	}

	identify() {
		let self = this
		api.usersIdentify()
			.then((res) => {
				self.fullName = res.data.fullName
				localStorage.setItem(nameHeader, self.fullName)
			})
			.catch(this.logout)
	}

	logout() {
		this.token = ''
		this.fullName = ''
		this.isAuthenticated = false
		localStorage.removeItem(tokenHeader)
		localStorage.removeItem(nameHeader)
	}

	// TODO: проверки разрешений на клиенте ()
}

const auth = new Auth()

export { auth, tokenHeader }
