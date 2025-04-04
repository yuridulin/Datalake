import api from '@/api/swagger-api'
import { AccessType, UserInfo } from '@/api/swagger/data-contracts'
import AccessTypeEl from '@/app/components/atomic/AccessTypeEl'
import UserButton from '@/app/components/buttons/UserButton'
import FormRow from '@/app/components/FormRow'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import getUserTypeName from '@/functions/getUserTypeName'
import { useInterval } from '@/hooks/useInterval'
import { timeAgo } from '@/state/timeAgoInstance'
import { user } from '@/state/user'
import { ClockCircleOutlined } from '@ant-design/icons'
import { Button, Input, Table, TableColumnsType, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const UsersList = observer(() => {
	const navigate = useNavigate()
	const [users, setUsers] = useState([] as UserInfo[])
	const [states, setStates] = useState({} as { [key: string]: string })
	const [search, setSearch] = useState('')

	const load = () => {
		api.usersReadAll().then((res) => setUsers(res.data))
		getStates()
	}

	const create = () => {
		navigate(routes.users.create)
	}

	const getStates = () => {
		if (!user.hasGlobalAccess(AccessType.Manager)) return
		api.systemGetVisits().then((res) => setStates(res.data))
	}

	const columns: TableColumnsType<UserInfo> = [
		{
			dataIndex: 'guid',
			title: 'Учетная запись',
			render: (_, record) => <UserButton userInfo={record} />,
		},
		{
			dataIndex: 'accessType',
			title: 'Глобальный уровень доступа',
			width: '16em',
			render: (_, record) => <AccessTypeEl type={record.accessType} />,
		},
		{
			dataIndex: 'accessType',
			title: 'Активность',
			width: '14em',
			render: (_, record) => {
				const state = states[record.guid]
				if (!state) return <></>
				const lastVisit = new Date(state)
				const late = Date.now() - Number(lastVisit)
				return (
					<Tag
						icon={<ClockCircleOutlined />}
						color={late < 120000 ? 'success' : late < 3600000 ? 'warning' : 'default'}
					>
						{timeAgo.format(lastVisit)}
					</Tag>
				)
			},
		},
		{
			dataIndex: 'guid',
			title: 'Тип учетной записи',
			width: '20em',
			render: (_, record) => <>{getUserTypeName(record.type)}</>,
		},
	]

	useEffect(load, [])
	useInterval(getStates, 5000)

	return (
		<>
			<PageHeader
				right={user.hasGlobalAccess(AccessType.Manager) && <Button onClick={create}>Добавить пользователя</Button>}
			>
				Список пользователей
			</PageHeader>
			{users.length > 0 && (
				<>
					<FormRow title='Поиск'>
						<Input
							value={search}
							onChange={(e) => setSearch(e.target.value)}
							placeholder='введите поисковый запрос...'
						/>
					</FormRow>
					<Table
						size='small'
						dataSource={users.filter((x) =>
							((x.login ?? '') + (x.fullName ?? '')).toLowerCase().trim().includes(search.toLowerCase()),
						)}
						columns={columns}
						rowKey='guid'
					/>
				</>
			)}
		</>
	)
})

export default UsersList
