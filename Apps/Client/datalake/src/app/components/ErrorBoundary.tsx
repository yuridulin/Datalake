import { Navigate, useRouteError } from 'react-router-dom'
import notify from '../../state/notifications'
import routes from '../router/routes'

export default function ErrorBoundary() {
	const error = useRouteError() as Error

	notify.err(error.message)

	return <Navigate to={routes.globalRoot} />
}
