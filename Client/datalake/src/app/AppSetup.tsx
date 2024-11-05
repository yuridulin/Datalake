import { ConfigProvider, theme } from 'antd'
import dayjs from 'dayjs'
import { useState } from 'react'
import { AuthProvider } from 'react-oidc-context'
import { RouterProvider } from 'react-router-dom'
import { UpdateContext } from '../context/updateContext'
import router from './router/router'
import routes from './router/routes'

import locale from 'antd/locale/ru_RU'
import 'dayjs/locale/ru'
import { observer } from 'mobx-react-lite'
import { user } from '../state/user'
dayjs.locale('ru')

declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string
declare const INSTANCE_NAME: string

if (INSTANCE_NAME) {
	document.title = 'Datalake | ' + INSTANCE_NAME
}

const AppSetup = observer(() => {
	const [lastUpdate, setUpdate] = useState<Date>(new Date())
	const { defaultAlgorithm, darkAlgorithm } = theme

	const oidcConfig = {
		authority:
			window.location.protocol + '//' + KEYCLOAK_DB + '/realms/energo',
		redirect_uri: window.location.origin + routes.auth.energoId,
		client_id: KEYCLOAK_CLIENT,
	}
	dayjs.locale('')

	return (
		<ConfigProvider
			theme={{
				algorithm: user.isDark() ? darkAlgorithm : defaultAlgorithm,
			}}
			locale={locale}
		>
			<UpdateContext.Provider value={{ lastUpdate, setUpdate }}>
				<AuthProvider {...oidcConfig}>
					<RouterProvider router={router} />
				</AuthProvider>
			</UpdateContext.Provider>
		</ConfigProvider>
	)
})

export default AppSetup
