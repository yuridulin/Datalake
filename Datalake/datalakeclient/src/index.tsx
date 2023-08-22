import React from 'react'
import ReactDOM from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import router from './router/router'
import axios from 'axios'
import { notification } from 'antd'

axios.defaults.baseURL = window.location.protocol + '//' + window.location.hostname + ':83/api'
axios.interceptors.response.use(
	res => {
		if (res.data.Done) {
			notification.info({ placement: 'bottomLeft', message: res.data.Done })
		}
		if (res.data.Error) {
			notification.error({ placement: 'bottomLeft', message: res.data.Error })
		}
		return res
	},
	err => {
		notification.error({ placement: 'bottomLeft', message: err.message })
		return Promise.reject(err)
	}
)

ReactDOM
	.createRoot(document.getElementById('root') as HTMLElement)
	.render(
		<React.StrictMode>
			<RouterProvider router={router} />
		</React.StrictMode>
	)
