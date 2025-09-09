import routes from '@/app/router/routes'
import { useAppStore } from '@/store/useAppStore'
import { App, ConfigProvider, notification, theme } from 'antd'
import locale from 'antd/locale/ru_RU'
import { observer } from 'mobx-react-lite'
import { useEffect } from 'react'
import { RouterProvider } from 'react-router-dom'
import { Initializing } from './pages/Initializing'
import { Offline } from './pages/Offline'
import AppRouter from './router/appRouter'

export const Datalake = observer(() => {
	const store = useAppStore()
	const { defaultAlgorithm, darkAlgorithm } = theme
	const [api, contextHolder] = notification.useNotification()

	useEffect(() => store.setNotify(api), [api, store])

	const isKeycloakCallback = window.location.href.includes(routes.auth.keycloak)
	if (isKeycloakCallback) {
		store.setConnectionStatus(true)
		store.doneLoading()
	}

	return (
		<ConfigProvider
			theme={{
				algorithm: store.isDark ? darkAlgorithm : defaultAlgorithm,
			}}
			locale={locale}
		>
			<App>
				{contextHolder}
				{store.isLoading ? <Initializing /> : store.isConnected ? <RouterProvider router={AppRouter} /> : <Offline />}
			</App>
		</ConfigProvider>
	)
})
