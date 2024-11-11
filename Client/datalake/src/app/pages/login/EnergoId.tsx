import api, { identifyUser } from '@/api/swagger-api'
import { Button, Space } from 'antd'
import { observer } from 'mobx-react-lite'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import notify from '../../../state/notifications'
import { user } from '../../../state/user'
import routes from '../../router/routes'

const EnergoId = observer(() => {
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
					user.setName(res.data.fullName)
					navigate(routes.globalRoot)
					identifyUser()
				}
			})
			.catch(() => {
				notify.err('Аутентификация не пройдена')
				navigate(routes.auth.loginPage)
			})

		return (
			<Space>
				Вы аутентифицированы как {auth.user?.profile.name}
				<Button onClick={() => void auth.removeUser()}>Выйти</Button>
			</Space>
		)
	}

	return <i>Что-то пошло не так</i>
})

export default EnergoId
