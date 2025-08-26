import { handleCallback } from '@/app/router/auth/keycloak/keycloakService'
import routes from '@/app/router/routes'
import { useAppStore } from '@/store/useAppStore'
import { Alert, Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const KeycloakCallback = observer(() => {
	const navigate = useNavigate()
	const store = useAppStore()
	const [error, setError] = useState<string | undefined>(undefined)
	const processedRef = useRef(false)

	useEffect(() => {
		if (processedRef.current) return
		processedRef.current = true

		const processAuth = async () => {
			const result = await handleCallback()
			console.log(result)

			if (result.success) {
				store.loginKeycloak(result.user!.sub, result.user!.email ?? '', result.user!.name ?? '')
				navigate(routes.globalRoot, {})
			} else {
				setError(result.error)
			}
		}

		processAuth()
	}, [store, navigate])

	if (error) {
		return (
			<Alert
				message='Ошибка аутентификации'
				description={error}
				type='error'
				action={
					<Button size='small' onClick={() => navigate(routes.auth.login)}>
						Вернуться к логину
					</Button>
				}
			/>
		)
	}

	return <Spin size='large'>Keycloak Callback</Spin>
})

export default KeycloakCallback
