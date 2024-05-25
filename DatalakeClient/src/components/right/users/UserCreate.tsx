import { Button, Input, Radio } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../../api/api'
import {
	AccessType,
	UserCreateRequest,
} from '../../../api/swagger/data-contracts'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

export default function UserCreate() {
	const navigate = useNavigate()
	const [user, setUser] = useState({
		accessType: AccessType.NOT,
		password: '',
		staticHost: '',
	} as UserCreateRequest)
	const [isStatic, setStatic] = useState(false)

	function create() {
		api.usersCreate(user).then(() => {
			navigate('/users/')
		})
	}

	return (
		<>
			<Header
				left={
					<Button onClick={() => navigate('/users/')}>
						Вернуться
					</Button>
				}
				right={
					<Button type='primary' onClick={create}>
						Создать
					</Button>
				}
			>
				Новая учётная запись
			</Header>
			<form>
				<FormRow title='Имя учётной записи'>
					<Input
						value={user.loginName}
						onChange={(e) =>
							setUser({ ...user, loginName: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Имя пользователя'>
					<Input
						value={user.fullName ?? ''}
						onChange={(e) =>
							setUser({ ...user, fullName: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Тип доступа'>
					<Radio.Group
						buttonStyle='solid'
						value={isStatic}
						onChange={(e) => setStatic(e.target.value)}
					>
						<Radio.Button value={false}>Базовый</Radio.Button>
						<Radio.Button value={true}>Статичный</Radio.Button>
					</Radio.Group>
				</FormRow>
				{isStatic ? (
					<FormRow title='Адрес, с которого разрешен доступ'>
						<Input
							value={user.staticHost || ''}
							onChange={(e) =>
								setUser({ ...user, staticHost: e.target.value })
							}
						/>
					</FormRow>
				) : (
					<FormRow title='Пароль'>
						<Input.Password
							value={user.password ?? ''}
							autoComplete='password'
							onChange={(e) =>
								setUser({ ...user, password: e.target.value })
							}
						/>
					</FormRow>
				)}
				<FormRow title='Тип учётной записи'>
					<Radio.Group
						buttonStyle='solid'
						value={user.accessType}
						onChange={(e) =>
							setUser({ ...user, accessType: e.target.value })
						}
					>
						<Radio.Button value={AccessType.NOT}>
							Отключена
						</Radio.Button>
						<Radio.Button value={AccessType.USER}>
							Пользователь
						</Radio.Button>
						<Radio.Button value={AccessType.ADMIN}>
							Администратор
						</Radio.Button>
					</Radio.Group>
				</FormRow>
			</form>
		</>
	)
}
