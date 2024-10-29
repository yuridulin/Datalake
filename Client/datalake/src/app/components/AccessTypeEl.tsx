import { Tag } from 'antd'
import getAccessTypeName from '../../api/models/getAccessTypeName'
import { AccessType } from '../../api/swagger/data-contracts'

export default function AccessTypeEl({
	type,
	bordered = true,
}: {
	type: AccessType
	bordered?: boolean
}) {
	const name = getAccessTypeName(type)
	switch (type) {
		case AccessType.NoAccess:
			return (
				<Tag color='volcano' bordered={bordered}>
					{name}
				</Tag>
			)
		case AccessType.Viewer:
			return (
				<Tag color='gold' bordered={bordered}>
					{name}
				</Tag>
			)
		case AccessType.User:
			return (
				<Tag color='green' bordered={bordered}>
					{name}
				</Tag>
			)
		case AccessType.Admin:
			return (
				<Tag color='blue' bordered={bordered}>
					{name}
				</Tag>
			)
		default:
			return <Tag bordered={bordered}>{name}</Tag>
	}
}
