import { Button, Table, Tag } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { NavLink } from 'react-router-dom'
import { AccessRightsForOneInfo } from '../../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../../components/AccessTypeEl'
import routes from '../../../../router/routes'

type ObjectsWithAccessProps = {
	accessRights: AccessRightsForOneInfo[]
}

const columns: ColumnsType<AccessRightsForOneInfo> = [
	{
		title: 'Объект',
		dataIndex: 'id',
		render: (_, record) => {
			if (record.source)
				return (
					<NavLink to={routes.sources.toEditSource(record.source.id)}>
						<Button size='small'>{record.source.name}</Button>
					</NavLink>
				)
			if (record.block)
				return (
					<NavLink to={routes.blocks.toViewBlock(record.block.id)}>
						<Button size='small'>{record.block.name}</Button>
					</NavLink>
				)
			if (record.tag)
				return (
					<NavLink to={routes.tags.toTag(record.tag.guid)}>
						<Button size='small'>{record.tag.name}</Button>
					</NavLink>
				)
			return <></>
		},
	},
	{
		title: 'Тип',
		dataIndex: 'id',
		render: (_, record) => {
			if (record.source) return <Tag>Источник</Tag>
			if (record.block) return <Tag>Блок</Tag>
			if (record.tag) return <Tag>Тег</Tag>
			return <></>
		},
	},
	{
		title: 'Уровень доступа',
		dataIndex: 'accessType',
		render: (type) => <AccessTypeEl type={type} />,
	},
]

export default function ObjectsWithAccess({
	accessRights,
}: ObjectsWithAccessProps) {
	return (
		<Table
			size='small'
			rowKey='id'
			columns={columns}
			dataSource={accessRights}
		/>
	)
}
