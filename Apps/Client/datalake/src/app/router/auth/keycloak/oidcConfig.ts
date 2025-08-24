import routes from '@/app/router/routes'

// мы передаем с сервера определенные в настройках в БД данные о сервере keycloak для внешней авторизации
declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string

let keycloakDb: string | null = null
let keycloakClient: string | null = null

try {
	keycloakDb = KEYCLOAK_DB
} catch (e) {
	console.log('LOCAL_API is not defined - set', null)
}
try {
	keycloakClient = KEYCLOAK_CLIENT
} catch (e) {
	console.log('KEYCLOAK_CLIENT is not defined - set', null)
}

// настройки keycloak
const oidcConfig = {
	authority: window.location.protocol + '//' + keycloakDb + '/realms/energo',
	redirect_uri: window.location.origin + routes.auth.keycloak,
	client_id: keycloakClient,
	response_type: 'code',
	scope: 'openid profile email',
}

const handleKeycloakLogin = () => {
	const url =
		`${oidcConfig.authority}/protocol/openid-connect/auth?` +
		`client_id=${oidcConfig.client_id}&` +
		`redirect_uri=${encodeURIComponent(oidcConfig.redirect_uri)}&` +
		`response_type=${oidcConfig.response_type}&` +
		`scope=${encodeURIComponent(oidcConfig.scope)}`

	window.location.href = url
}

export { handleKeycloakLogin, oidcConfig }
