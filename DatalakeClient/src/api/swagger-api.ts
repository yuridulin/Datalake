import { notification } from 'antd'
import { AxiosError, AxiosResponse } from 'axios'
import { auth, tokenHeader } from '../etc/auth'
import router from '../router/router'
import { Api } from './swagger/Api'

declare const LOCAL_API: boolean

let isLocal = false
try {
	isLocal = LOCAL_API
} catch (e) {}

const api = new Api({
	baseURL: !!isLocal ? window.location.href : 'https://localhost:32781/',
	validateStatus(status) {
		return status >= 200 && status < 300
	},
})

api.instance.interceptors.request.use(
	function (config) {
		config.headers[tokenHeader] = auth.getSessionToken()
		return config
	},
	function (error) {
		return Promise.reject(error)
	},
)

api.instance.interceptors.response.use(
	function (res: AxiosResponse) {
		// запросы, которые не обрабатываем
		if (res.config.method === 'OPTIONS') {
			return res
		}
		// переход на логин, если нет доступа
		else if (res.status === 403) {
			router.navigate('/login')
		}
		// обработка ошибки с сервера
		else if (res.status >= 300) {
			throw new AxiosError(res.data, String(res.status), res.config)
		}
		// нормальное развитие событий
		else {
			// данные о доступе сохраняем
			auth.setSessionToken(res.headers[tokenHeader])

			// сообщения после выполнения действий
			if (res.status === 204) {
				notification.success({
					placement: 'bottomLeft',
					message: 'Успешно',
				})
			} else if (res.data.Done) {
				notification.info({
					placement: 'bottomLeft',
					message: res.data.Done,
				})
			} else if (res.data.Error) {
				notification.error({
					placement: 'bottomLeft',
					message: res.data.Error,
				})
			}
		}
		return res
	},
	function (err: AxiosError) {
		// переход на ожидание соединения, если не получилось провести запрос
		if (err.request?.status === 0) {
			return router.navigate('/offline')
		}
		// переход на логин, если нет доступа
		else if (err.response?.status === 403) {
			return router.navigate('/login')
		}
		// сообщения после выполнения действий
		else {
			try {
				let message = err.request?.responseText as string
				if (message.indexOf('\n\n') > -1)
					message = message.substring(0, message.indexOf('\n\n'))
				return notification.error({
					placement: 'bottomLeft',
					message: message,
				})
			} catch (e) {
				return notification.error({
					placement: 'bottomLeft',
					message: 'Ошибка выполнения',
				})
			}
		}
	},
)

export default api
