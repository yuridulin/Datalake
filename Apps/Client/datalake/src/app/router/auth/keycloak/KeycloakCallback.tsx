import { useAppStore } from '@/store/useAppStore'
import { Button, Space } from 'antd'
import { observer } from 'mobx-react-lite'
import { useAuth } from 'react-oidc-context'

const KeycloakCallback = observer(() => {
	const auth = useAuth()
	const store = useAppStore()

	console.log('keycloak callback', auth)

	if (auth.isLoading) {
		return <div>загрузка...</div>
	}

	if (auth.error) {
		return <div>Ошибка при аутентификации через EnergoId: {auth.error.message}</div>
	}

	if (auth.isAuthenticated) {
		store.loginKeycloak(auth.user?.profile.sub ?? '', auth.user?.profile.email ?? '', auth.user?.profile.name ?? '')

		return (
			<Space>
				Вы аутентифицированы как {auth.user?.profile.name}
				<Button onClick={() => void auth.removeUser()}>Выйти</Button>
			</Space>
		)
	}

	return <i>Что-то пошло не так</i>
})

export default KeycloakCallback
