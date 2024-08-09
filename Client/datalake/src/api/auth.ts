const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'

const isAuth = () => localStorage.getItem(tokenHeader) !== ''

const getToken = () => localStorage.getItem(tokenHeader)
const setToken = (token: string) => localStorage.setItem(tokenHeader, token)
const freeToken = () => localStorage.setItem(tokenHeader, '')

const getName = () => localStorage.getItem(nameHeader)
const setName = (name: string) => localStorage.setItem(nameHeader, name)

export {
	freeToken,
	getName,
	getToken,
	isAuth,
	nameHeader,
	setName,
	setToken,
	tokenHeader,
}
