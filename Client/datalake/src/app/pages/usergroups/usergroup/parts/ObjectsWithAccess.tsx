import BlockButton from '@/app/components/buttons/BlockButton'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import { Table, Tag } from 'antd'
import { ColumnsType } from 'antd/es/table'
import {
	AccessRightsForOneInfo,
	AccessType,
} from '../../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../../components/AccessTypeEl'

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
			if (record.source) return <SourceButton source={record.source} />
			if (record.block) return <BlockButton block={record.block} />
			if (record.tag) return <TagButton tag={record.tag} />
			return <>?</>
		},
	},
]

const ObjectsWithAccess = ({ accessRights }: ObjectsWithAccessProps) => {
	return (
		<Table
			size='small'
			rowKey='id'
			columns={columns}
			dataSource={accessRights}
		/>
	)
}

export default ObjectsWithAccess
