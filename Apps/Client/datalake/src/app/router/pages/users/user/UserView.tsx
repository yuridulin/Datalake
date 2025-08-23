import api from '@/api/swagger-api'
import PageHeader from '@/app/components/PageHeader'
import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import InfoTable from '@/app/components/infoTable/InfoTable'
import TabsView from '@/app/components/tabsView/TabsView'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserDetailInfo } from '@/generated/data-contracts'
import { user } from '@/state/user'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'

const UserView = observer(() => {
	const navigate = useNavigate()
	const { id } = useParams()
	const [info, setInfo] = useState(null as UserDetailInfo | null)
	const [loading, setLoading] = useState(true)

	const load = () => {
		if (!id) return
		setLoading(true)
		api.usersGetWithDetails(String(id)).then((res) => {
			setInfo(res.data)
			setLoading(false)
		})
	}

	useEffect(load, [id])

	return loading ? (
		<Spin />
	) : info ? (
		<>
			<PageHeader
				left={<Button onClick={() => navigate(-1)}>К предыдущей странице</Button>}
				right={
					<>
						{user.hasGlobalAccess(AccessType.Admin) && <Button disabled>Редактировать разрешения</Button>}
						&ensp;
						{hasAccess(info.accessRule.access, AccessType.Manager) && (
							<NavLink to={routes.users.toUserForm(info.guid)}>
								<Button>Редактировать учетную запись</Button>
							</NavLink>
						)}
					</>
				}
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
