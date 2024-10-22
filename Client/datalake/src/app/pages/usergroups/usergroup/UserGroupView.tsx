import { Button, Descriptions, DescriptionsProps, Spin, Tabs } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'
import api from '../../../../api/swagger-api'
import {
	AccessType,
	UserGroupDetailedInfo,
} from '../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../components/AccessTypeEl'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'
import ObjectsWithAccess from './parts/ObjectsWithAccess'
import SubgroupsUsersTable from './parts/SubgroupsUsersTable'

const defaultGroup = {} as UserGroupDetailedInfo

export default function UserGroupView() {
	const [group, setGroup] = useState(defaultGroup)
	const [ready, setReady] = useState(false)
	const { id } = useParams()

	const getGlobalAccessRights = () => {
		if (!group.accessRights) return AccessType.NotSet
		const parent = group.accessRights.filter((x) => x.isGlobal)
		const accessType =
			parent.length > 0 ? parent[0].accessType : AccessType.NotSet
		return accessType ?? AccessType.NotSet
	}

	const items: DescriptionsProps['items'] = [
		{
			key: 'desc',
			label: 'Описание',
			children: group.description,
		},
		{
			key: 'access',
			label: 'Общий уровень доступа',
			children: <AccessTypeEl type={getGlobalAccessRights()} />,
		},
	]

	const load = () => {
		setReady(false)
		if (!id) return
		api.userGroupsReadWithDetails(id)
			.then((res) => {
				if (res.data?.guid) {
					setGroup(res.data)
					setReady(true)
				}
			})
			.catch(() => setGroup(defaultGroup))
	}

	const checkReady = () => {
		setReady(!!group.guid)
	}

	useEffect(load, [id])
	useEffect(checkReady, [group])

	return !ready ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.userGroups.root}>
						<Button>К списку групп</Button>
					</NavLink>
				}
				right={
					<>
						<NavLink
							to={routes.userGroups.toUserGroupEdit(String(id))}
						>
							<Button>Редактирование группы и участников</Button>
						</NavLink>
						&ensp;
						<NavLink
							to={routes.userGroups.toUserGroupAccessForm(
								String(id),
							)}
						>
							<Button>Редактирование разрешений</Button>
						</NavLink>
					</>
				}
			>
				{group.name}
			</PageHeader>
			<Descriptions colon={true} items={items} />
			<Tabs
				items={[
					{
						key: '1',
						label: 'Подгруппы и участники',
						children: (
							<SubgroupsUsersTable
								subgroups={group.subgroups}
								users={group.users}
								guid={group.guid}
								onCreateGroup={load}
							/>
						),
					},
					{
						key: '2',
						label: 'Разрешения',
						children: (
							<ObjectsWithAccess
								accessRights={group.accessRights.filter(
									(x) => !x.isGlobal,
								)}
							/>
						),
					},
				]}
			/>
		</>
	)
}
