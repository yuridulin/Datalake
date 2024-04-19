import { AccessType } from '../api/data-contracts'

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'

const auth = {
	name: (_?: string) =>
		!!_
			? localStorage.setItem(nameHeader, _)
			: localStorage.getItem(nameHeader) || '',
	token: (_?: string) =>
		!!_
			? localStorage.setItem(tokenHeader, _)
			: localStorage.getItem(tokenHeader) || '0',
	access: (_?: AccessType) =>
		!!_
			? localStorage.setItem(accessHeader, String(_))
			: localStorage.getItem(accessHeader) || AccessType.NOT,
	isAdmin() {
		return this.access() === AccessType.ADMIN
	},
}

export { accessHeader, auth, nameHeader, tokenHeader }
