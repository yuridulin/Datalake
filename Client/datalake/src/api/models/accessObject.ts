export enum AccessObject {
	UserGroup = 0,
	User = 1,
	Source = 10,
	Block = 11,
	Tag = 12,
}

export function AccessObjectFromString(value: string | undefined) {
	switch (value) {
		case 'user-group':
			return AccessObject.UserGroup
		case 'user':
			return AccessObject.User
		case 'source':
			return AccessObject.Source
		case 'block':
			return AccessObject.Block
		case 'tag':
			return AccessObject.Tag
		default:
			return null
	}
}

export function AccessObjectToString(value: AccessObject) {
	switch (value) {
		case AccessObject.UserGroup:
			return 'user-group'
		case AccessObject.User:
			return 'user'
		case AccessObject.Source:
			return 'source'
		case AccessObject.Block:
			return 'block'
		case AccessObject.Tag:
			return 'tag'
		default:
			return '?'
	}
}

export function AccessObjectName(value: AccessObject | null) {
	switch (value) {
		case AccessObject.UserGroup:
			return 'Группа пользователей'
		case AccessObject.User:
			return 'Пользователь'
		case AccessObject.Source:
			return 'Источник'
		case AccessObject.Block:
			return 'Блок'
		case AccessObject.Tag:
			return 'Тег'
		default:
			return '?'
	}
}
