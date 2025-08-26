import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import UserIcon from '@/app/components/icons/UserIcon'
import InfoTable from '@/app/components/infoTable/InfoTable'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserDetailInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'

const UserView = observer(() => {
	const store = useAppStore()
	const navigate = useNavigate()
	const { id } = useParams()
	const [info, setInfo] = useState(null as UserDetailInfo | null)
	const [loading, setLoading] = useState(true)

	const load = () => {
		if (!id) return
		setLoading(true)
		store.api.usersGetWithDetails(String(id)).then((res) => {
			setInfo(res.data)
			setLoading(false)
		})
	}

	useEffect(load, [store, id])

	return loading ? (
		<Spin />
	) : info ? (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(-1)}>К предыдущей странице</Button>]}
				right={[
					store.hasGlobalAccess(AccessType.Admin) && <Button disabled>Редактировать разрешения</Button>,
					hasAccess(info.accessRule.access, AccessType.Manager) && (
						<NavLink to={routes.users.toUserForm(info.guid)}>
							<Button>Редактировать учетную запись</Button>
						</NavLink>
					),
				]}
				icon={<UserIcon />}
			>
				Учётная запись: {info.fullName}
			</PageHeader>

			<InfoTable
				items={{
					'Полное имя': info.fullName,
					'Тип учетной записи': getUserTypeName(info.type),
				}}
			/>
			<br />

			<TabsView
				items={[
					{
						key: 'groups',
						label: 'Группы',
						children:
							info.userGroups.length > 0 ? (
								info.userGroups.map((record) => (
									<div style={{ marginBottom: '1em' }} key={record.guid}>
										<UserGroupButton group={record} />
									</div>
								))
							) : (
								<i>нет</i>
							),
					},
				]}
			/>
		</>
	) : (
		<></>
	)
})

export default UserView
