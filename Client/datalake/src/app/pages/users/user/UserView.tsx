import api from '@/api/swagger-api'
import {
	AccessType,
	UserDetailInfo,
	UserGroupSimpleInfo,
} from '@/api/swagger/data-contracts'
import PageHeader from '@/app/components/PageHeader'
import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import hasAccess from '@/functions/hasAccess'
import { user } from '@/state/user'
import { Button, Descriptions, Divider, Spin, Table } from 'antd'
import Column from 'antd/es/table/Column'
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
		api.usersReadWithDetails(String(id)).then((res) => {
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
				left={
					<Button onClick={() => navigate(-1)}>
						К предыдущей странице
					</Button>
				}
				right={
					<>
						{user.hasGlobalAccess(AccessType.Admin) && (
							<Button disabled>Редактировать разрешения</Button>
						)}
						&ensp;
						{hasAccess(
							info.accessRule.accessType,
							AccessType.Manager,
						) && (
							<NavLink to={routes.users.toUserForm(info.guid)}>
								<Button>Редактировать учетную запись</Button>
							</NavLink>
						)}
					</>
				}
			>
				Учётная запись: {info.fullName}
			</PageHeader>
			<Descriptions
				items={[
					{
						key: 'name',
						label: 'Полное имя',
						children: info.fullName,
					},
					{
						key: 'type',
						label: 'Тип учетной записи',
						children: getUserTypeName(info.type),
					},
				]}
			/>
			<Divider orientation='left'>
				<small>Группы</small>
			</Divider>
			<Table size='small' dataSource={info.userGroups} rowKey='guid'>
				<Column
					title='Группа'
					render={(_, record: UserGroupSimpleInfo) => (
						<UserGroupButton group={record} />
					)}
				/>
			</Table>
		</>
	) : (
		<></>
	)
})

export default UserView
