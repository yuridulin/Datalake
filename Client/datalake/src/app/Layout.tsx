import { ConfigProvider, theme } from 'antd'
import { useEffect, useState } from 'react'
import { AuthProvider } from 'react-oidc-context'
import { RouterProvider } from 'react-router-dom'
import { UpdateContext } from '../context/updateContext'
import router from './router/router'
import routes from './router/routes'

declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string

export default function Layout() {
	const [lastUpdate, setUpdate] = useState<Date>(new Date())
	const { defaultAlgorithm, darkAlgorithm } = theme
	const [isDarkMode, setIsDarkMode] = useState(false)

	const oidcConfig = {
		authority:
			window.location.protocol + '//' + KEYCLOAK_DB + '/realms/energo',
		redirect_uri: window.location.origin + routes.Auth.EnergoId,
		client_id: KEYCLOAK_CLIENT,
	}

	useEffect(() => {
		setIsDarkMode(
			window.matchMedia &&
				window.matchMedia('(prefers-color-scheme: dark)').matches,
		)
		window
			.matchMedia('(prefers-color-scheme: dark)')
			.addEventListener('change', (event) => {
				setIsDarkMode(event.matches)
			})
	}, [])

	return (
		<ConfigProvider
			theme={{
				algorithm: isDarkMode ? darkAlgorithm : defaultAlgorithm,
			}}
		>
			<UpdateContext.Provider value={{ lastUpdate, setUpdate }}>
				<AuthProvider {...oidcConfig}>
					<RouterProvider router={router} />
				</AuthProvider>
			</UpdateContext.Provider>
		</ConfigProvider>
	)
}
