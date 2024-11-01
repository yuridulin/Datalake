import { AxiosError, AxiosResponse } from 'axios'
import router from '../app/router/router'
import routes from '../app/router/routes'
import notify from './notifications'
import { Api } from './swagger/Api'
import { AccessType } from './swagger/data-contracts'
import { globalAccessHeader, nameHeader, tokenHeader, user } from './user'

declare const LOCAL_API: boolean

let isLocal = false
try {
	isLocal = LOCAL_API
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
} catch (e) {
	/* empty */
}

const api = new Api({
	baseURL: isLocal ? window.location.origin + '/' : 'http://localhost:8000/',
	validateStatus(status) {
		return status >= 200 && status < 300
	},
})

api.instance.interceptors.request.use(
	(config) => {
		config.headers[tokenHeader] = user.token
		return config
	},
	(err) => Promise.reject(err),
)

api.instance.interceptors.response.use(
	(response: AxiosResponse) => {
		if (response.config.method === 'OPTIONS') return response

		user.setName(decodeURIComponent(response.headers[nameHeader]))
		user.setToken(response.headers[tokenHeader])
		user.setAccess(
			response.headers[globalAccessHeader] || AccessType.NoAccess,
		)

		if (response.status === 204) {
			notify.done()
		}

		return response
	},
	(error: AxiosError) => {
		if (error.response?.status === 401) {
			router.navigate(routes.auth.loginPage, { replace: true })
			return Promise.reject(error.response)
		}

		if (error.code === 'ERR_NETWORK') {
			router.navigate(routes.offline, { replace: true })
			return Promise.resolve(error.response)
		}

		if (error.response?.status === 403) {
			return Promise.reject(error.response)
		}

		if (error.response?.status ?? 0 >= 500) {
			let message = error.request?.responseText as string
			if (message.indexOf('\n\n') > -1)
				message = message.substring(0, message.indexOf('\n\n'))
			notify.err(message)
			return Promise.reject(error.response)
		}

		return Promise.reject(error)
	},
)

if (window.location.pathname !== routes.auth.energoId) {
	api.usersIdentify()
		.then((res) => {
			if (res.status != 200) return
			user.identify(res.data)
		})
		.catch()
}

export default api
