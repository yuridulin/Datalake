import getAccessTypeName from '@/functions/getAccessTypeName'
import { AccessType } from '@/generated/data-contracts'
import { Tag } from 'antd'

export default function AccessTypeEl({ type, bordered = true }: { type: AccessType; bordered?: boolean }) {
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
		case AccessType.Editor:
			return (
				<Tag color='lime' bordered={bordered}>
					{name}
				</Tag>
			)
		case AccessType.Manager:
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
