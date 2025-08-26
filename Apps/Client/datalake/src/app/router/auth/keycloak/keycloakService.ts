import routes from '@/app/router/routes'
import { appStore } from '@/store/appStore'
import { UserManager, WebStorageStateStore } from 'oidc-client-ts'

// мы передаем с сервера определенные в настройках в БД данные о сервере keycloak для внешней авторизации
declare const KEYCLOAK_DB: string
declare const KEYCLOAK_CLIENT: string

let keycloakDb: string = 'null'
let keycloakClient: string = 'null'

try {
	keycloakDb = KEYCLOAK_DB
} catch {
	console.log('LOCAL_API is not defined - set', null)
}
try {
	keycloakClient = KEYCLOAK_CLIENT
} catch {
	console.log('KEYCLOAK_CLIENT is not defined - set', null)
}

// настройки keycloak
const settings = {
	authority: window.location.protocol + '//' + keycloakDb + '/realms/energo',
	redirect_uri: window.location.origin + routes.auth.keycloak,
	client_id: keycloakClient,
	response_type: 'code',
	scope: 'openid profile email',
	userStore: new WebStorageStateStore({ store: window.localStorage }),
}

export const userManager = new UserManager(settings)

// Функция для входа
export const loginWithKeycloak = () => {
	appStore.doneLoading()
	userManager.signinRedirect()
}

// Функция для обработки callback
export const handleCallback = async () => {
	try {
		const user = await userManager.signinRedirectCallback()
		return {
			success: true,
			user: {
				sub: user.profile.sub,
				email: user.profile.email,
				name: user.profile.name,
			},
		}
	} catch (error) {
		return {
			success: false,
			error: (error as { message: string }).message,
		}
	}
}

// Функция для выхода
export const logout = () => {
	userManager.signoutRedirect()
}
