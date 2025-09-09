import routes from '@/app/router/routes'
import { AccessType, UserGroupInfo, UserGroupSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { NavLink } from 'react-router-dom'
import UserGroupsCreateModal from '../modals/UserGroupsCreateModal'

type SubgroupsTableProps = {
	guid: string
	subgroups: UserGroupSimpleInfo[]
	onCreateGroup: () => void
}

const SubgroupsTable = ({ guid, subgroups, onCreateGroup }: SubgroupsTableProps) => {
	const store = useAppStore()
	return (
		<>
			{store.hasAccessToGroup(AccessType.Manager, guid) && (
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
