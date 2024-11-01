import { AccessType } from '../swagger/data-contracts'

export default function getAccessTypeName(type: AccessType) {
	switch (type) {
		case AccessType.NoAccess:
			return 'Нет доступа'
		case AccessType.Viewer:
			return 'Просмотр'
		case AccessType.User:
			return 'Изменение'
		case AccessType.Admin:
			return 'Полный доступ'
		case AccessType.NotSet:
			return 'Не установлен'
		default:
			return '?'
	}
}
