import ReactDOM from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import router from './router/router'
import axios, { AxiosError, AxiosResponse } from 'axios'
import { notification } from 'antd'
import { accessHeader, auth, loginHeader, tokenHeader } from './etc/auth'

// настройка взаимодействия с сервером

axios.defaults.baseURL = window.location.protocol + '//' + window.location.hostname + ':83/api'

axios.interceptors.request.use(
	function (config) {
		// добавляем информацию о пользователе ко всем запросам
		config.headers[loginHeader] = auth.name()
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
			auth.name(res.headers[loginHeader])
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
		else {
			console.log('server action error:', err.message)
			return notification.error({ placement: 'bottomLeft', message: err.message })
			//return Promise.reject(err)
		}
	}
)

ReactDOM
	.createRoot(document.getElementById('root') as HTMLElement)
	.render(
		<RouterProvider router={router} />
	)
