import { Button, notification, Space } from 'antd'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { setName } from '../../../api/local-auth'
import api from '../../../api/swagger-api'
import routes from '../../router/routes'

export default function EnergoId() {
	const auth = useAuth()
	const navigate = useNavigate()

	if (auth.isLoading) {
		return <div>загрузка...</div>
	}

	if (auth.error) {
		return (
			<div>
				Ошибка при аутентификации через EnergoId: {auth.error.message}
			</div>
		)
	}

	if (auth.isAuthenticated) {
		api.usersAuthenticateEnergoIdUser({
			energoIdGuid: auth.user?.profile.sub ?? '',
			login: auth.user?.profile.email ?? '',
			fullName: auth.user?.profile.name ?? '',
		})
			.then((res) => {
				if (res.status === 200) {
					setName(res.data.fullName)
					navigate(routes.Root)
				}
			})
			.catch(() => {
				notification.error({
					placement: 'bottomLeft',
					message: 'Аутентификация не пройдена',
				})
				navigate(routes.Auth.LoginPage)
			})

		return (
			<Space>
				Вы аутентифицированы как {auth.user?.profile.name}
				<Button onClick={() => void auth.removeUser()}>Выйти</Button>
			</Space>
		)
	}

	return <i>Что-то пошло не так</i>
}
