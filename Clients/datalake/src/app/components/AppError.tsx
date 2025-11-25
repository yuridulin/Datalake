import { Button, Result } from 'antd'
import { ErrorResponse, isRouteErrorResponse, useNavigate, useRouteError } from 'react-router-dom'
import { HomeOutlined, ReloadOutlined } from '@ant-design/icons'

const AppError = () => {
	const error = useRouteError()
	const navigate = useNavigate()

	let errorMessage: string = ''
	let statusCode: number | undefined

	if (isRouteErrorResponse(error)) {
		// error is type `ErrorResponse`
		statusCode = error.status
		errorMessage = (error as ErrorResponse).data?.message || error.statusText || `Ошибка ${error.status}`
	} else if (error instanceof Error) {
		errorMessage = error.message || 'Произошла ошибка при загрузке страницы'
	} else if (typeof error === 'string') {
		errorMessage = error
	} else {
		// TODO: Заменить на logger после реализации пункта 1.2
		console.error('Unknown route error:', error)
		errorMessage = 'Произошла неизвестная ошибка'
	}

	const handleGoHome = () => {
		navigate('/')
	}

	const handleReload = () => {
		window.location.reload()
	}

	return (
		<div style={{ padding: '24px', minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
			<Result
				status={statusCode === 404 ? '404' : statusCode === 403 ? '403' : 'error'}
				title={statusCode ? `Ошибка ${statusCode}` : 'Ошибка маршрутизации'}
				subTitle={errorMessage}
				extra={[
					<Button
						type="primary"
						key="home"
						icon={<HomeOutlined />}
						onClick={handleGoHome}
					>
						На главную
					</Button>,
					<Button
						key="reload"
						icon={<ReloadOutlined />}
						onClick={handleReload}
					>
						Перезагрузить страницу
					</Button>,
				]}
			/>
		</div>
	)
}

export default AppError
