import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import UserIcon from '@/app/components/icons/UserIcon'
import InfoTable from '@/app/components/infoTable/InfoTable'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import { AccessType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'

const UserView = observer(() => {
	const store = useAppStore()
	const navigate = useNavigate()
	const { id } = useParams()
	useDatalakeTitle('Пользователи', id)
	// Получаем пользователя из store (реактивно через MobX)
	const info = id ? store.usersStore.getUserByGuid(id) : undefined
	const isLoading = id ? store.usersStore.isLoadingUser(id) : false

	// Загружаем данные пользователя при первом монтировании или изменении id
	useEffect(() => {
		if (id) {
			store.usersStore.refreshUserByGuid(id)
		}
	}, [id, store.usersStore])

	return isLoading && !info ? (
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
