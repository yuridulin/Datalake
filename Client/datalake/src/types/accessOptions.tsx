import { AccessType } from '@/api/swagger/data-contracts'
import AccessTypeEl from '@/app/components/atomic/AccessTypeEl'
import { DefaultOptionType } from 'antd/es/select'

export const accessOptions: DefaultOptionType[] = [
	{
		label: <AccessTypeEl type={AccessType.NotSet} bordered={false} />,
		value: AccessType.NotSet,
	},
	{
		label: <AccessTypeEl type={AccessType.NoAccess} bordered={false} />,
		value: AccessType.NoAccess,
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
