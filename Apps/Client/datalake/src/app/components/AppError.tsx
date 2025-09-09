import { Alert } from 'antd'
import { ErrorResponse, isRouteErrorResponse, useRouteError } from 'react-router-dom'

const { ErrorBoundary } = Alert

const AppError = () => {
	const error = useRouteError()
	let errorMessage: string = ''

	if (isRouteErrorResponse(error)) {
		// error is type `ErrorResponse`
		errorMessage = (error as ErrorResponse).data?.message || error.statusText
	} else if (error instanceof Error) {
		errorMessage = error.message
	} else if (typeof error === 'string') {
		errorMessage = error
	} else {
		console.error(error)
		errorMessage = 'Unknown error'
	}
	return <ErrorBoundary>{errorMessage}</ErrorBoundary>
}

export default AppError
