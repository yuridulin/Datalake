import { user } from '@/state/user'
import { Button, Divider, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import Column from 'antd/es/table/Column'
import { NavLink } from 'react-router-dom'
import {
	AccessType,
	UserGroupInfo,
	UserGroupSimpleInfo,
	UserGroupUsersInfo,
} from '../../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../../components/AccessTypeEl'
import routes from '../../../../router/routes'
import UserGroupsCreateModal from '../modals/UserGroupsCreateModal'

type SubgroupsUsersTableProps = {
	guid: string
	subgroups: UserGroupSimpleInfo[]
	users: UserGroupUsersInfo[]
	onCreateGroup: () => void
}

const columns: ColumnsType<UserGroupUsersInfo> = [
	{
		title: 'Имя',
		dataIndex: 'name',
		render: (_, record) =>
			!record.guid ? (
				<>?</>
			) : (
				<NavLink to={routes.users.toUserView(record.guid)}>
					<Button size='small'>{record.fullName}</Button>
				</NavLink>
			),
	},
	{
		title: 'Разрешение на доступ к группе',
		dataIndex: 'access',
		render: (_, record) => <AccessTypeEl type={record.accessType} />,
		sorter: (a, b) => (a > b ? 1 : -1),
		width: '35%',
	},
]

const SubgroupsUsersTable = ({
	guid,
	subgroups,
	users,
	onCreateGroup,
}: SubgroupsUsersTableProps) => {
	return (
		<>
			<Divider>
				<small>Участники</small>
			</Divider>
			<Table
				size='small'
				rowKey='guid'
				columns={columns}
				dataSource={users}
			/>

			<Divider
				variant='dashed'
				orientation='left'
				style={{ fontSize: '1em' }}
			>
				Подгруппы
				{user.hasAccessToGroup(AccessType.Manager, guid) && (
					<>
						&emsp;
						<UserGroupsCreateModal
							isSmall={true}
							onCreate={onCreateGroup}
							parentGuid={guid}
						/>
					</>
				)}
			</Divider>
			{subgroups.length > 0 ? (
				<Table
					dataSource={subgroups}
					size='small'
					pagination={false}
					rowKey='guid'
				>
					<Column<UserGroupInfo>
						dataIndex='id'
						title='Название'
						render={(_, record) => (
							<NavLink
								to={routes.userGroups.toUserGroup(record.guid)}
							>
								<Button size='small'>{record.name}</Button>
							</NavLink>
						)}
					/>
				</Table>
			) : (
				<i>Нет подгрупп</i>
			)}
		</>
	)
}

export default SubgroupsUsersTable
