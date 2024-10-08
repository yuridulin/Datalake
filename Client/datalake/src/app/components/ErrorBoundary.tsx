import { notification } from 'antd'
import { Navigate, useRouteError } from 'react-router-dom'
import routes from '../router/routes'

export default function ErrorBoundary() {
	const error = useRouteError() as Error

	notification.error({
		message: error.message,
	})

	return <Navigate to={routes.globalRoot} />
}
