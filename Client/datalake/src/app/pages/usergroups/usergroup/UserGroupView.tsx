import api from '@/api/swagger-api'
import AccessTypeEl from '@/app/components/atomic/AccessTypeEl'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import SubgroupsTable from '@/app/pages/usergroups/usergroup/parts/SubgroupsTable'
import { user } from '@/state/user'
import { Button, Descriptions, DescriptionsProps, Spin, Tabs } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'
import { AccessType, UserGroupDetailedInfo } from '../../../../api/swagger/data-contracts'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'
import { default as MembersTable } from './parts/MembersTable'
import ObjectsWithAccess from './parts/ObjectsWithAccess'

const defaultGroup = {} as UserGroupDetailedInfo

type UserGroupTabs = 'members' | 'nested' | 'access' | 'logs'

const UserGroupView = observer(() => {
	const [group, setGroup] = useState(defaultGroup)
	const [ready, setReady] = useState(false)
	const [activeTab, setActiveTab] = useState<UserGroupTabs>('members')
	const { id } = useParams()

	const getGlobalAccessRights = () => {
		if (!group.accessRights) return AccessType.NotSet
		const parent = group.accessRights.filter((x) => x.isGlobal)
		const accessType = parent.length > 0 ? parent[0].accessType : AccessType.NotSet
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
		api
			.userGroupsReadWithDetails(id)
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

	useEffect(() => {
		const hash = window.location.hash.replace('#', '')
		if (hash) {
			setActiveTab(hash as UserGroupTabs)
		}
	}, [])

	useEffect(() => {
		const hash = window.location.hash.replace('#', '')
		if (hash && hash != activeTab) {
			setActiveTab(hash as UserGroupTabs)
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [window.location.hash])

	const onTabChange = (key: string) => {
		setActiveTab(key as UserGroupTabs)
		window.location.hash = key // Обновление хэша в URL
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
							<NavLink to={routes.userGroups.toUserGroupEdit(String(id))}>
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
			<Descriptions colon={true} items={items} />
			<Tabs
				activeKey={activeTab}
				onChange={onTabChange}
				animated={false}
				destroyInactiveTabPane
				tabBarStyle={{ height: '100%' }}
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
