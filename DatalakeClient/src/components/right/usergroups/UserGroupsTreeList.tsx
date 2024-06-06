import { Button, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { UserGroupInfo } from '../../../api/swagger/data-contracts'
import routeLinks from '../../../router/routeLinks'
import Header from '../../small/Header'
import UserGroupsCreateModal from './UserGroupsCreateModal'

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
				<NavLink to={routeLinks.toUserGroup(record.guid)}>
					<Button size='small'>{record.name}</Button>
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
	let row = {
		name: group.name,
		guid: group.userGroupGuid,
		description: group.description,
	} as UserGroupRow

	let childs = flat.filter((x) => x.parentGroupGuid === group.userGroupGuid)
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

	return (
		<>
			<Header right={<UserGroupsCreateModal onCreate={load} />}>
				Группы пользователей
			</Header>
			<Table
				columns={columns}
				dataSource={groups}
				size='small'
				rowKey='guid'
			/>
		</>
	)
}
