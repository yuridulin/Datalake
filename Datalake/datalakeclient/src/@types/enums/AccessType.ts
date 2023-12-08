export enum AccessType {
	FIRST = -1,
	NOT = 0,
	USER = 1,
	ADMIN = 2,
}

export function AccessTypeDescription(type: AccessType) {
	switch (type) {
		case AccessType.FIRST: return 'Впервые'
		case AccessType.NOT: return 'Нет доступа'
		case AccessType.USER: return 'Пользователь'
		case AccessType.ADMIN: return 'Администратор'
		default: return '?'
	}
}