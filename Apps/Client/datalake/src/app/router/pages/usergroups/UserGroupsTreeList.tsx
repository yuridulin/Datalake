import AccessTypeEl from '@/app/components/AccessTypeEl'
import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import PageHeader from '@/app/components/PageHeader'
import { AccessType, UserGroupTreeInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useLocalStorage } from 'react-use'
import routes from '../../routes'
import UserGroupsCreateModal from './usergroup/modals/UserGroupsCreateModal'

const setEmptyAsLeafs = (group: UserGroupTreeInfo): UserGroupTreeInfo => ({
	...group,
	children: group.children.length > 0 ? group.children.map((x) => setEmptyAsLeafs(x)) : (undefined as never),
})

const UserGroupsTreeList = observer(() => {
	const store = useAppStore()
	const [groups, setGroups] = useState([] as UserGroupTreeInfo[])

	const load = () => {
		store.api.userGroupsGetTree().then((res) => {
			setGroups(res.data.map((x) => setEmptyAsLeafs(x)))
		})
	}

	useEffect(load, [store.api])

	const expandKey = 'expandedUserGroups'
	const [expandedRowKeys, setExpandedRowKeys] = useLocalStorage(expandKey, [] as string[])

	const onExpand = (expanded: boolean, record: UserGroupTreeInfo) => {
		const exists = expandedRowKeys ?? []
		const keys = expanded ? [...exists, record.guid] : exists.filter((key) => key !== record.guid)
		setExpandedRowKeys(keys)
	}

	const columns: ColumnsType<UserGroupTreeInfo> = [
		{
			title: 'Имя',
			dataIndex: 'name',
			key: 'name',
			render(_, record) {
				return <UserGroupButton group={record} />
			},
		},
		{
			title: 'Описание',
			dataIndex: 'description',
			key: 'description',
		},
		{
			title: 'Общий уровень доступа',
			dataIndex: 'globalAccessType',
			key: 'globalAccessType',
			render(value) {
				return <AccessTypeEl type={value} />
			},
		},
	]

	return (
		<>
			<PageHeader
				right={
					store.hasGlobalAccess(AccessType.Manager)
						? [
								<NavLink to={routes.userGroups.move}>
									<Button>Изменить иерархию</Button>
								</NavLink>,
								<UserGroupsCreateModal onCreate={load} />,
							]
						: []
				}
			>
				Группы пользователей
			</PageHeader>
			<Table
				columns={columns}
				dataSource={groups}
				expandable={{
					expandedRowKeys,
					onExpand,
				}}
				size='small'
				rowKey='guid'
			/>
		</>
	)
})

export default UserGroupsTreeList
