import { user } from '@/state/user'
import { Button, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { NavLink } from 'react-router-dom'
import { AccessType, UserGroupInfo, UserGroupSimpleInfo } from '../../../../../api/swagger/data-contracts'
import routes from '../../../../router/routes'
import UserGroupsCreateModal from '../modals/UserGroupsCreateModal'

type SubgroupsTableProps = {
	guid: string
	subgroups: UserGroupSimpleInfo[]
	onCreateGroup: () => void
}

const SubgroupsTable = ({ guid, subgroups, onCreateGroup }: SubgroupsTableProps) => {
	return (
		<>
			{user.hasAccessToGroup(AccessType.Manager, guid) && (
				<div style={{ marginBottom: '1em' }}>
					<UserGroupsCreateModal isSmall={true} onCreate={onCreateGroup} parentGuid={guid} />
				</div>
			)}
			{subgroups.length > 0 ? (
				<Table dataSource={subgroups} size='small' pagination={false} rowKey='guid'>
					<Column<UserGroupInfo>
						dataIndex='id'
						title='Название'
						render={(_, record) => (
							<NavLink to={routes.userGroups.toViewUserGroup(record.guid)}>
								<Button size='small'>{record.name}</Button>
							</NavLink>
						)}
					/>
				</Table>
			) : (
				<i>Нет дочерних групп</i>
			)}
		</>
	)
}

export default SubgroupsTable
