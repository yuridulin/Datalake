import AccessTypeEl from '@/app/components/AccessTypeEl'
import UserButton from '@/app/components/buttons/UserButton'
import { Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { AccessType, UserGroupUsersInfo } from '../../../../../api/swagger/data-contracts'

type MembersTableProps = {
	users: UserGroupUsersInfo[]
}

const columns: ColumnsType<UserGroupUsersInfo> = [
	{
		title: 'Имя',
		dataIndex: 'name',
		render: (_, record) =>
			record ? (
				<UserButton
					userInfo={{
						guid: record.guid,
						fullName: record.fullName || '?',
						accessRule: { ruleId: 0, access: AccessType.Viewer },
					}}
				></UserButton>
			) : (
				<>?</>
			),
	},
	{
		title: 'Уровень доступа к управлению группой и подгруппами',
		dataIndex: 'access',
		render: (_, record) => <AccessTypeEl type={record.accessType} />,
		sorter: (a, b) => (a > b ? 1 : -1),
		width: '35%',
	},
]

const MembersTable = ({ users }: MembersTableProps) => {
	return <Table size='small' rowKey='guid' columns={columns} dataSource={users} />
}

export default MembersTable
