import { AccessType } from '../api/swagger/data-contracts'

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
			: localStorage.getItem(accessHeader) || AccessType.NoAccess,
	isAdmin() {
		return this.access() === AccessType.Admin
	},
}

export { accessHeader, auth, nameHeader, tokenHeader }
