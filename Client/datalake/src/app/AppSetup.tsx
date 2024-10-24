import { ConfigProvider, theme } from 'antd'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import { AuthProvider } from 'react-oidc-context'
import { RouterProvider } from 'react-router-dom'
import { UpdateContext } from '../context/updateContext'
import router from './router/router'
import routes from './router/routes'

import locale from 'antd/locale/ru_RU'
import 'dayjs/locale/ru'
import appTheme from '../api/theme-settings'
dayjs.locale('ru')

declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string
declare const INSTANCE_NAME: string

if (INSTANCE_NAME) {
	document.title = 'Datalake | ' + INSTANCE_NAME
}

export default function AppSetup() {
	const [lastUpdate, setUpdate] = useState<Date>(new Date())
	const { defaultAlgorithm, darkAlgorithm } = theme
	const [isDarkMode, setDarkMode] = useState(false)

	const oidcConfig = {
		authority:
			window.location.protocol + '//' + KEYCLOAK_DB + '/realms/energo',
		redirect_uri: window.location.origin + routes.auth.energoId,
		client_id: KEYCLOAK_CLIENT,
	}

	useEffect(() => {
		setDarkMode(appTheme.isDark())
	}, [])
	useEffect(() => {
		appTheme.setTheme(isDarkMode)
	}, [isDarkMode])
	dayjs.locale('')

	return (
		<ConfigProvider
			theme={{
				algorithm: isDarkMode ? darkAlgorithm : defaultAlgorithm,
			}}
			locale={locale}
		>
			<UpdateContext.Provider
				value={{ lastUpdate, setUpdate, isDarkMode, setDarkMode }}
			>
				<AuthProvider {...oidcConfig}>
					<RouterProvider router={router} />
				</AuthProvider>
			</UpdateContext.Provider>
		</ConfigProvider>
	)
}
