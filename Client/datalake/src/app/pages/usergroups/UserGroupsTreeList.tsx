import { Button, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { UserGroupInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'
import UserGroupsCreateModal from './usergroup/modals/UserGroupsCreateModal'

type UserGroupRow = {
	guid: string
	name: string
	description?: string | null | undefined
	children?: UserGroupRow[]
}

const columns: ColumnsType<UserGroupRow> = [
	{
		title: 'Имя',
		dataIndex: 'name',
		key: 'name',
		render(value, record, index) {
			return (
				<NavLink
					key={index}
					to={routes.userGroups.toUserGroup(record.guid)}
				>
					<Button size='small' title={value}>
						{record.name}
					</Button>
				</NavLink>
			)
		},
	},
	{
		title: 'Описание',
		dataIndex: 'description',
		key: 'description',
	},
]

function userGroupToRow(
	group: UserGroupInfo,
	flat: UserGroupInfo[],
): UserGroupRow {
	const row = {
		name: group.name,
		guid: group.guid,
		description: group.description,
	} as UserGroupRow

	const childs = flat.filter((x) => x.parentGroupGuid === group.guid)
	if (childs.length > 0) {
		row.children = childs.map((x) => userGroupToRow(x, flat))
	}

	return row
}

export default function UserGroupsTreeList() {
	const [groups, setGroups] = useState([] as UserGroupRow[])

	function load() {
		api.userGroupsReadAll().then((res) => {
			const flat = res.data
			setGroups(
				flat
					.filter((x) => !x.parentGroupGuid)
					.map((x) => userGroupToRow(x, flat)),
			)
		})
	}

	useEffect(load, [])

	const expandKey = 'expandedUserGroups'
	const [expandedRowKeys, setExpandedRowKeys] = useState(() => {
		const savedKeys = localStorage.getItem(expandKey)
		return savedKeys
			? (JSON.parse(savedKeys) as string[])
			: ([] as string[])
	})

	const onExpand = (expanded: boolean, record: UserGroupRow) => {
		const keys = expanded
			? [...expandedRowKeys, record.guid]
			: expandedRowKeys.filter((key) => key !== record.guid)
		setExpandedRowKeys(keys)
	}

	useEffect(() => {
		localStorage.setItem(expandKey, JSON.stringify(expandedRowKeys))
	}, [expandedRowKeys])

	return (
		<>
			<PageHeader
				right={
					<>
						<NavLink to={routes.userGroups.move}>
							<Button>Изменить иерархию</Button>
						</NavLink>
						&ensp;
						<UserGroupsCreateModal onCreate={load} />
					</>
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
}
