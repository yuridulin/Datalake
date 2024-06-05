import { notification } from 'antd'
import { AxiosError, AxiosResponse } from 'axios'
import { accessHeader, auth, nameHeader, tokenHeader } from '../etc/auth'
import router from '../router/router'
import { Api } from './swagger/Api'

const api = new Api({
	validateStatus(status) {
		return status >= 200 && status < 300
	},
})

api.instance.interceptors.request.use(
	function (config) {
		//console.log('send request:', config.url)
		// добавляем информацию о пользователе ко всем запросам
		/* console.log('send auth info', {
			nameHeader: auth.name(),
			tokenHeader: auth.token(),
			accessHeader: auth.access(),
		}) */
		config.headers[nameHeader] = auth.name()
		config.headers[tokenHeader] = auth.token()
		config.headers[accessHeader] = auth.access()
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
			//console.log('finish request:', res.config.url)
			//console.log('headers', res.headers)
			// данные о доступе сохраняем
			auth.name(res.headers[nameHeader])
			auth.token(res.headers[tokenHeader])
			auth.access(res.headers[accessHeader])
			/* console.log('update auth info', {
				nameHeader: auth.name(),
				tokenHeader: auth.token(),
				accessHeader: auth.access(),
			}) */

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
			//console.log('is offline')
			return router.navigate('/offline')
		}
		// переход на логин, если нет доступа
		else if (err.response?.status === 403) {
			//console.log('not authorized')
			return router.navigate('/login')
		}
		// сообщения после выполнения действий
		else {
			console.log('server action error:', err)
			try {
				let message = err.request?.responseText as string
				if (message.indexOf('\n\n') > -1)
					message = message.substring(0, message.indexOf('\n\n'))
				return notification.error({
					placement: 'bottomLeft',
					message: message,
				})
			} catch (e) {
				console.error(e)
				return notification.error({
					placement: 'bottomLeft',
					message: 'Ошибка выполнения',
				})
			}
		}
	},
)

export default api
