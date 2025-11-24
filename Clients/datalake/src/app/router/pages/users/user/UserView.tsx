import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import UserIcon from '@/app/components/icons/UserIcon'
import InfoTable from '@/app/components/infoTable/InfoTable'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import { AccessType, UserWithGroupsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useRef, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'

const UserView = observer(() => {
	const store = useAppStore()
	const navigate = useNavigate()
	const { id } = useParams()
	useDatalakeTitle('Пользователи', id)
	const [info, setInfo] = useState(null as UserWithGroupsInfo | null)
	const [loading, setLoading] = useState(true)
	const hasLoadedRef = useRef(false)
	const lastIdRef = useRef<string | undefined>(id)

	useEffect(() => {
		// Если изменился id, сбрасываем флаг загрузки
		if (lastIdRef.current !== id) {
			hasLoadedRef.current = false
			lastIdRef.current = id
		}

		if (hasLoadedRef.current || !id) return
		hasLoadedRef.current = true

		setLoading(true)
		store.api.inventoryUsersGetWithDetails(String(id)).then((res) => {
			setInfo(res.data)
			setLoading(false)
		})
	}, [store.api, id])

	return loading ? (
		<Spin />
	) : info ? (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(-1)}>К предыдущей странице</Button>]}
				right={[
					store.hasGlobalAccess(AccessType.Admin) && <Button disabled>Редактировать разрешения</Button>,
					store.hasGlobalAccess(AccessType.Manager) && (
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
