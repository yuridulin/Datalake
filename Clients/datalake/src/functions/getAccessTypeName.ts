import { AccessType } from '../generated/data-contracts'

export default function getAccessTypeName(type: AccessType) {
	switch (type) {
		case AccessType.None:
			return 'Базовый'
		case AccessType.Denied:
			return 'Нет доступа'
		case AccessType.Viewer:
			return 'Просмотр'
		case AccessType.Editor:
			return 'Изменение'
		case AccessType.Manager:
			return 'Менеджмент'
		case AccessType.Admin:
			return 'Полный доступ'
		default:
			return '?'
	}
}
