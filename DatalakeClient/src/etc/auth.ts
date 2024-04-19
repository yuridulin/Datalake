import { AccessType } from "../@types/enums/AccessType";

const nameHeader = 'd-name'
const tokenHeader = 'd-access-token'
const accessHeader = 'd-access-type'

const auth = {
	name: (_?: string) => !!_ ? localStorage.setItem(nameHeader, _) : (localStorage.getItem(nameHeader) || ''),
	token: (_?: string) => !!_ ? localStorage.setItem(tokenHeader, _) : (localStorage.getItem(tokenHeader) || '0'),
	access: (_?: AccessType) => !!_ ? localStorage.setItem(accessHeader, String(_)) : Number(localStorage.getItem(accessHeader) || AccessType.NOT) as AccessType,
	isAdmin() {
		return this.access() === AccessType.ADMIN
	},
}

export {
	auth,
	nameHeader,
	tokenHeader,
	accessHeader,
}