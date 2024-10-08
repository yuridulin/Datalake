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
dayjs.locale('ru')

declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string

const datalakeThemeKey = 'datalake-theme'
const datalakeThemeDark = 'dark'
const datalakeThemeLight = 'light'

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
		const ls = localStorage.getItem(datalakeThemeKey)
		const storedMode =
			ls == null
				? window.matchMedia &&
				  window.matchMedia('(prefers-color-scheme: dark)').matches
				: ls == datalakeThemeDark
		setDarkMode(storedMode)

		/* window
			.matchMedia('(prefers-color-scheme: dark)')
			.addEventListener('change', (event) => {
				if (localStorage.getItem('datalake-dark-mode') == null)
					setDarkMode(event.matches)
			}) */
	}, [])
	useEffect(
		() =>
			localStorage.setItem(
				datalakeThemeKey,
				isDarkMode ? datalakeThemeDark : datalakeThemeLight,
			),
		[isDarkMode],
	)

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
