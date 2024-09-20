import { Button, Input, Table, TableColumnsType } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	AccessType,
	UserInfo,
	UserType,
} from '../../../api/swagger/data-contracts'
import FormRow from '../../components/FormRow'
import Header from '../../components/Header'

function AccessTypeDescription(type?: AccessType) {
	switch (type) {
		case AccessType.Admin:
			return 'администратор'
		case AccessType.User:
			return 'пользователь'
		case AccessType.Viewer:
			return 'наблюдатель'
		default:
			return 'нет доступа'
	}
}

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
		title: 'Уровень доступа',
		render: (_, record) => <>{AccessTypeDescription(record.accessType)}</>,
	},
	{
		dataIndex: 'guid',
		title: 'Тип учетной записи',
		render: (_, record) => <>{UserTypeDescription(record.type)}</>,
	},
]

export default function UsersList() {
	const navigate = useNavigate()
	const [users, setUsers] = useState([] as UserInfo[])
	const [search, setSearch] = useState('')

	function load() {
		api.usersReadAll().then((res) => setUsers(res.data))
	}

	function create() {
		navigate('/users/create')
	}

	useEffect(load, [])

	return (
		<>
			<Header
				right={<Button onClick={create}>Добавить пользователя</Button>}
			>
				Список пользователей
			</Header>
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
							(
								(x.login ?? '') +
								(x.fullName ?? '') +
								AccessTypeDescription(x.accessType)
							)
								.toLowerCase()
								.trim()
								.includes(search.toLowerCase()),
						)}
						columns={columns}
					/>
				</>
			)}
		</>
	)
}
