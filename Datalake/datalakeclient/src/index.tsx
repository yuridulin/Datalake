import ReactDOM from 'react-dom/client'
import router from './router/router'
import axios, { AxiosError, AxiosResponse } from 'axios'
import { notification } from 'antd'
import { accessHeader, auth, nameHeader, tokenHeader } from './etc/auth'
import Layout from './components/Layout'

// настройка взаимодействия с сервером

declare const PORT: number // переменная, которую создает сервер в отдельном файле

axios.defaults.baseURL = window.location.protocol + '//' + window.location.hostname + ':' + PORT + '/api'

axios.interceptors.request.use(
	function (config) {
		// добавляем информацию о пользователе ко всем запросам
		config.headers[nameHeader] = auth.name()
		config.headers[tokenHeader] = auth.token()
		config.headers[accessHeader] = auth.access()
		return config
	},
	function (error) {
		return Promise.reject(error)
	}
)

axios.interceptors.response.use(
	function (res: AxiosResponse) {
		// запросы, которые не обрабатываем
		if (res.config.method === 'OPTIONS') {
			return res
		}
		// переход на логин, если нет доступа
		else if (res.status === 403) {
			router.navigate('/login')
		}
		// нормальное развитие событий
		else {
			// данные о доступе сохраняем
			auth.name(res.headers[nameHeader])
			auth.token(res.headers[tokenHeader])
			auth.access(res.headers[accessHeader])

			// сообщения после выполнения действий
			if (res.data.Done) {
				notification.info({ placement: 'bottomLeft', message: res.data.Done })
			}
			else if (res.data.Error) {
				notification.error({ placement: 'bottomLeft', message: res.data.Error })
			}
		}
		return res
	},
	function (err: AxiosError) {
		console.log('error interceptor', err)

		// переход на ожидание соединения, если не получилось провести запрос
		if (err.request?.status === 0) {
			console.log('is offline')
			return router.navigate('/offline')
		}
		// переход на логин, если нет доступа
		else if (err.response?.status === 403) {
			console.log('not authorized')
			return router.navigate('/login')
		}
		// сообщения после выполнения действий
		else if (err.request?.status === 500) {
			return notification.error({ placement: 'bottomLeft', message: (err.response?.data as any)?.Error })
		}
		// сообщения о транспортной ошибке
		else {
			console.log('server action error:', err.message)
			return notification.error({ placement: 'bottomLeft', message: err.message })
		}
	}
)

ReactDOM
	.createRoot(document.getElementById('root') as HTMLElement)
	.render(<Layout />)
