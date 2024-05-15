import { Button, Input } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import api from '../../../api/api'
import { AccessType, UserInfo } from '../../../api/swagger/data-contracts'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

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

	function AccessTypeDescription(type?: AccessType) {
		switch (type) {
			case AccessType.ADMIN:
				return 'администратор'
			case AccessType.USER:
				return 'пользователь'
			default:
				return 'нет доступа'
		}
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
					<div className='table'>
						<div className='table-header'>
							<span style={{ width: '12em' }}>Логин</span>
							<span style={{ width: '12em' }}>
								Уровень доступа
							</span>
							<span
								style={{ width: '12em' }}
								title='Статичный тип доступа используются для обращения без необходимости логиниться'
							>
								Тип доступа
							</span>
							<span>Имя</span>
						</div>
						{users
							.filter((x) =>
								(
									(x.loginName ?? '') +
									(x.fullName ?? '') +
									AccessTypeDescription(x.accessType)
								)
									.toLowerCase()
									.trim()
									.includes(search.toLowerCase()),
							)
							.map((x) => (
								<div className='table-row' key={x.loginName}>
									<span>
										<NavLink to={'/users/' + x.loginName}>
											<Button>{x.loginName}</Button>
										</NavLink>
									</span>
									<span>
										{AccessTypeDescription(x.accessType)}
									</span>
									<span>
										{!!x.isStatic ? 'статичный' : 'базовый'}
									</span>
									<span>{x.fullName}</span>
								</div>
							))}
					</div>
				</>
			)}
		</>
	)
}
