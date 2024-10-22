import { Button, Table, Tag } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { NavLink } from 'react-router-dom'
import {
	AccessRightsForOneInfo,
	AccessType,
} from '../../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../../components/AccessTypeEl'
import BlockIcon from '../../../../icons/BlockIcon'
import SourceIcon from '../../../../icons/SourceIcon'
import TagIcon from '../../../../icons/TagIcon'
import routes from '../../../../router/routes'

type ObjectsWithAccessProps = {
	accessRights: AccessRightsForOneInfo[]
}

const columns: ColumnsType<AccessRightsForOneInfo> = [
	{
		title: 'Тип объекта',
		width: '10em',
		render: (_, record) => {
			if (record.source) return <Tag>Источник</Tag>
			if (record.block) return <Tag>Блок</Tag>
			if (record.tag) return <Tag>Тег</Tag>
			return <>?</>
		},
		sorter: (a, b) =>
			(a.source?.id ?? a.block?.id ?? a.tag?.id ?? 0) -
			(b.source?.id ?? b.block?.id ?? b.tag?.id ?? 0),
	},
	{
		title: 'Уровень доступа',
		dataIndex: 'accessType',
		width: '14em',
		render: (access: AccessType) => <AccessTypeEl type={access} />,
		sorter: (a, b) => Number(a.accessType) - Number(b.accessType),
	},
	{
		dataIndex: 'id',
		title: 'Объект',
		render: (_, record) => {
			if (record.source)
				return (
					<NavLink to={routes.sources.toViewSource(record.source.id)}>
						<Button size='small'>
							<SourceIcon /> {record.source.name}
						</Button>
					</NavLink>
				)
			if (record.block)
				return (
					<NavLink to={routes.blocks.toViewBlock(record.block.id)}>
						<Button size='small'>
							<BlockIcon /> {record.block.name}
						</Button>
					</NavLink>
				)
			if (record.tag)
				return (
					<NavLink to={routes.tags.toTag(record.tag.guid)}>
						<Button size='small'>
							<TagIcon /> {record.tag.name}
						</Button>
					</NavLink>
				)
			return <>?</>
		},
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
