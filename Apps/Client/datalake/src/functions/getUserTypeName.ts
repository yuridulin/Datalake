import { UserType } from '@/api/swagger/data-contracts'

export default function getUserTypeName(type: UserType) {
	switch (type) {
		case UserType.Local:
			return 'Локальная учётная запись'

		case UserType.Static:
			return 'Статичная учётная запись'

		case UserType.EnergoId:
			return 'Учётная запись EnergoID'
		default:
			return '?'
	}
}
