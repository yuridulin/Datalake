import { ClockCircleOutlined } from '@ant-design/icons'
import { Button, Input, Table, TableColumnsType, Tag } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { timeAgo } from '../../../api/extensions/timeAgoInstance'
import api from '../../../api/swagger-api'
import { UserInfo, UserType } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import AccessTypeEl from '../../components/AccessTypeEl'
import FormRow from '../../components/FormRow'
import PageHeader from '../../components/PageHeader'

function UserTypeDescription(type: UserType) {
	switch (type) {
		case UserType.Local:
			return 'Локальная учётная запись'

		case UserType.Static:
			return 'Статичная учётная запись'

		case UserType.EnergoId:
			return 'Учётная запись EnergoID'
		default:
			return '?'
	}
}

export default function UsersList() {
	const navigate = useNavigate()
	const [users, setUsers] = useState([] as UserInfo[])
	const [states, setStates] = useState({} as { [key: string]: string })
	const [search, setSearch] = useState('')

	const load = () => {
		api.usersReadAll().then((res) => setUsers(res.data))
		getStates()
	}

	const create = () => {
		navigate('/users/create')
	}

	const getStates = () => {
		api.systemGetVisits().then((res) => setStates(res.data))
	}

	const columns: TableColumnsType<UserInfo> = [
		{
			dataIndex: 'guid',
			title: 'Учетная запись',
			render: (_, record) => (
				<NavLink to={'/users/' + record.guid}>
					<Button size='small'>{record.fullName}</Button>
				</NavLink>
			),
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
				if (!state) return <Tag>не было</Tag>
				const lastVisit = new Date(state)
				const late = Date.now() - Number(lastVisit)
				return (
					<Tag
						icon={<ClockCircleOutlined />}
						color={
							late < 120000
								? 'success'
								: late < 3600000
								? 'warning'
								: 'default'
						}
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
			render: (_, record) => <>{UserTypeDescription(record.type)}</>,
		},
	]

	useEffect(load, [])
	useInterval(getStates, 5000)

	return (
		<>
			<PageHeader
				right={<Button onClick={create}>Добавить пользователя</Button>}
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
							((x.login ?? '') + (x.fullName ?? ''))
								.toLowerCase()
								.trim()
								.includes(search.toLowerCase()),
						)}
						columns={columns}
						rowKey='guid'
					/>
				</>
			)}
		</>
	)
}
