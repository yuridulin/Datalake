import AccessTypeEl from '@/app/components/AccessTypeEl'
import { AccessType } from '@/generated/data-contracts'
import { DefaultOptionType } from 'antd/es/select'

export const accessOptions: DefaultOptionType[] = [
	{
		label: <AccessTypeEl type={AccessType.None} bordered={false} />,
		value: AccessType.None,
	},
	{
		label: <AccessTypeEl type={AccessType.Denied} bordered={false} />,
		value: AccessType.Denied,
	},
	{
		label: <AccessTypeEl type={AccessType.Viewer} bordered={false} />,
		value: AccessType.Viewer,
	},
	{
		label: <AccessTypeEl type={AccessType.Editor} bordered={false} />,
		value: AccessType.Editor,
	},
	{
		label: <AccessTypeEl type={AccessType.Manager} bordered={false} />,
		value: AccessType.Manager,
	},
	{
		label: <AccessTypeEl type={AccessType.Admin} bordered={false} />,
		value: AccessType.Admin,
	},
]
