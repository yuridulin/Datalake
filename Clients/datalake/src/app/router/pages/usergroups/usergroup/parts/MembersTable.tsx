import AccessTypeEl from '@/app/components/AccessTypeEl'
import UserButton from '@/app/components/buttons/UserButton'
import { UserGroupMemberInfo } from '@/generated/data-contracts'
import { Table } from 'antd'
import { ColumnsType } from 'antd/es/table'

type MembersTableProps = {
	users: UserGroupMemberInfo[]
}

const columns: ColumnsType<UserGroupMemberInfo> = [
	{
		title: 'Имя',
		dataIndex: 'name',
		render: (_, record) => (record ? <UserButton userInfo={record} /> : <>?</>),
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
