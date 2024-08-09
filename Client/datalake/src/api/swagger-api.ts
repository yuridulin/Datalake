import { notification } from 'antd'
import { AxiosError, AxiosResponse } from 'axios'
import router from '../app/router/router'
import { getToken, setToken, tokenHeader } from './auth'
import { Api } from './swagger/Api'

declare const LOCAL_API: boolean

let isLocal = false
try {
	isLocal = LOCAL_API
} catch (e) {}

const api = new Api({
	baseURL: !!isLocal ? window.location.href : 'http://localhost:8000/',
	validateStatus(status) {
		return status >= 200 && status < 300
	},
})

api.instance.interceptors.request.use(
	(config) => {
		config.headers[tokenHeader] = getToken()
		return config
	},
	(err) => Promise.reject(err),
)

api.instance.interceptors.response.use(
	(response: AxiosResponse) => {
		if (response.config.method === 'OPTIONS') return response

		setToken(response.headers[tokenHeader])

		if (response.status === 204) {
			notification.success({
				placement: 'bottomLeft',
				message: 'Успешно',
			})
		}

		return response
	},
	(error: AxiosError) => {
		if (error.response?.status === 403) {
			router.navigate('/login')
			return Promise.resolve(error.response)
		}

		if (error.code === 'ERR_NETWORK') {
			router.navigate('/offline')
			return Promise.resolve(error.response)
		}

		if (error.response?.status ?? 0 >= 500) {
			let message = error.request?.responseText as string
			if (message.indexOf('\n\n') > -1)
				message = message.substring(0, message.indexOf('\n\n'))
			notification.error({
				placement: 'bottomLeft',
				message: message,
			})
			return Promise.resolve(error.response)
		}

		return Promise.reject(error)
	},
)

api.usersIdentify().catch()

export default api
