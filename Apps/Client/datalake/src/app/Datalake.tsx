import { oidcConfig } from '@/store/appStore'
import { useAppStore } from '@/store/useAppStore'
import { App, ConfigProvider, notification, theme } from 'antd'
import locale from 'antd/locale/ru_RU'
import { observer } from 'mobx-react-lite'
import { useEffect } from 'react'
import { AuthProvider } from 'react-oidc-context'
import { RouterProvider } from 'react-router-dom'
import { Initializing } from './pages/Initializing'
import { NotAuthorized } from './pages/NotAuthorized'
import { Offline } from './pages/Offline'
import AppRouter from './router/appRouter'

export const Datalake = observer(() => {
	const store = useAppStore()
	const { defaultAlgorithm, darkAlgorithm } = theme
	const [api, contextHolder] = notification.useNotification()

	useEffect(() => store.setNotify(api), [api])

	return (
		<ConfigProvider
			theme={{
				algorithm: store.isDark ? darkAlgorithm : defaultAlgorithm,
			}}
			locale={locale}
		>
			<App>
				{contextHolder}
				{store.isLoading ? (
					<Initializing />
				) : store.isConnected ? (
					<AuthProvider {...oidcConfig}>
						{store.isAuthenticated ? <RouterProvider router={AppRouter} /> : <NotAuthorized />}
					</AuthProvider>
				) : (
					<Offline />
				)}
			</App>
		</ConfigProvider>
	)
})
