import { ConfigProvider, theme } from 'antd'
import { KeycloakProvider } from 'keycloak-react-web'
import { useEffect, useState } from 'react'
import { RouterProvider } from 'react-router-dom'
import { keycloak } from '../api/keycloak'
import { UpdateContext } from '../context/updateContext'
import router from '../router/router'

export default function Layout() {
	const [lastUpdate, setUpdate] = useState<Date>(new Date())
	const [checkedTags, setCheckedTags] = useState<string[]>([])

	const { defaultAlgorithm, darkAlgorithm } = theme
	const [isDarkMode, setIsDarkMode] = useState(false)

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
			<UpdateContext.Provider
				value={{ lastUpdate, setUpdate, checkedTags, setCheckedTags }}
			>
				<KeycloakProvider client={keycloak as any}>
					<RouterProvider router={router} />
				</KeycloakProvider>
			</UpdateContext.Provider>
		</ConfigProvider>
	)
}
