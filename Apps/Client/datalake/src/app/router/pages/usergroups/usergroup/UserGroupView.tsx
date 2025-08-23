import api from '@/api/swagger-api'
import AccessTypeEl from '@/app/components/AccessTypeEl'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import TabsView from '@/app/components/tabsView/TabsView'
import SubgroupsTable from '@/app/pages/usergroups/usergroup/parts/SubgroupsTable'
import { user } from '@/state/user'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'
import { AccessType, UserGroupDetailedInfo } from '../../../../generated/data-contracts'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'
import { default as MembersTable } from './parts/MembersTable'
import ObjectsWithAccess from './parts/ObjectsWithAccess'

const defaultGroup = {} as UserGroupDetailedInfo

type UserGroupTabs = 'members' | 'nested' | 'access' | 'logs'

const UserGroupView = observer(() => {
	const [group, setGroup] = useState(defaultGroup)
	const [ready, setReady] = useState(false)
	const { id } = useParams()

	const items: InfoTableProps['items'] = {
		Описание: group.description ?? <i>нет</i>,
		'Общий уровень доступа группы': <AccessTypeEl type={group.globalAccessType} />,
	}

	const load = () => {
		setReady(false)
		if (!id) return
		api
			.userGroupsGetWithDetails(id)
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
						{user.hasAccessToGroup(AccessType.Editor, String(id)) && (
							<NavLink to={routes.userGroups.toEditUserGroup(String(id))}>
								<Button>Редактирование группы и участников</Button>
							</NavLink>
						)}
						&ensp;
						{user.hasGlobalAccess(AccessType.Admin) && (
							<NavLink to={routes.userGroups.toUserGroupAccessForm(String(id))}>
								<Button>Редактирование разрешений</Button>
							</NavLink>
						)}
					</>
				}
			>
				{group.name}
			</PageHeader>

			<InfoTable items={items} />
			<br />

			<TabsView
				items={[
					{
						key: 'members' as UserGroupTabs,
						label: 'Участники',
						children: <MembersTable users={group.users} />,
					},
					{
						key: 'nested' as UserGroupTabs,
						label: 'Дочерние группы',
						children: <SubgroupsTable subgroups={group.subgroups} guid={group.guid} onCreateGroup={load} />,
					},
					{
						key: 'access' as UserGroupTabs,
						label: 'Разрешения',
						children: <ObjectsWithAccess accessRights={group.accessRights.filter((x) => !x.isGlobal)} />,
					},
					{
						key: 'logs' as UserGroupTabs,
						label: 'События',
						children: <LogsTableEl userGroupGuid={group.guid} />,
					},
				]}
			/>
		</>
	)
})

export default UserGroupView
