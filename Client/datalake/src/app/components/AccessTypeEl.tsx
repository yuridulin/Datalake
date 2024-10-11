import { Tag } from 'antd'
import { AccessType } from '../../api/swagger/data-contracts'

export default function AccessTypeEl({ type }: { type: AccessType }) {
	switch (type) {
		case AccessType.NoAccess:
			return <Tag color='volcano'>Нет доступа</Tag>
		case AccessType.Viewer:
			return <Tag color='gold'>Просмотр</Tag>
		case AccessType.User:
			return <Tag color='green'>Изменение</Tag>
		case AccessType.Admin:
			return <Tag color='blue'>Полный доступ</Tag>
		case AccessType.NotSet:
			return <Tag>Не установлен</Tag>
		default:
			return <Tag>...</Tag>
	}
}
