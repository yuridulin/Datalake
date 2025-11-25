import { Alert, Button, Result } from 'antd'
import { Component, ErrorInfo, ReactNode } from 'react'
import { ReloadOutlined, HomeOutlined } from '@ant-design/icons'

interface Props {
	children: ReactNode
	fallback?: ReactNode
}

interface State {
	hasError: boolean
	error: Error | null
	errorInfo: ErrorInfo | null
}

class ErrorBoundary extends Component<Props, State> {
	constructor(props: Props) {
		super(props)
		this.state = {
			hasError: false,
			error: null,
			errorInfo: null,
		}
	}

	static getDerivedStateFromError(error: Error): Partial<State> {
		// Обновляем состояние, чтобы следующий рендер показал fallback UI
		return {
			hasError: true,
			error,
		}
	}

	componentDidCatch(error: Error, errorInfo: ErrorInfo) {
		// Логируем ошибку
		// TODO: Заменить на logger после реализации пункта 1.2
		console.error('ErrorBoundary caught an error:', error, errorInfo)

		this.setState({
			error,
			errorInfo,
		})

		// Здесь можно отправить ошибку в систему мониторинга
		// Например: logErrorToService(error, errorInfo)
	}

	handleReload = () => {
		// Перезагружаем страницу
		window.location.reload()
	}

	handleGoHome = () => {
		// Переходим на главную страницу
		window.location.href = '/'
	}

	handleReset = () => {
		// Сбрасываем состояние ошибки
		this.setState({
			hasError: false,
			error: null,
			errorInfo: null,
		})
	}

	render() {
		if (this.state.hasError) {
			// Если передан кастомный fallback, используем его
			if (this.props.fallback) {
				return this.props.fallback
			}

			// Показываем понятное сообщение об ошибке
			// Проверяем dev-режим через hostname (localhost) или наличие source maps
			const isDev =
				window.location.hostname === 'localhost' ||
				window.location.hostname === '127.0.0.1' ||
				!window.location.hostname
			const errorMessage = this.state.error?.message || 'Произошла непредвиденная ошибка'
			const errorStack = this.state.error?.stack
			const componentStack = this.state.errorInfo?.componentStack

			return (
				<div style={{ padding: '24px', minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
					<Result
						status="error"
						title="Что-то пошло не так"
						subTitle={errorMessage}
						extra={[
							<Button
								type="primary"
								key="reload"
								icon={<ReloadOutlined />}
								onClick={this.handleReload}
							>
								Перезагрузить страницу
							</Button>,
							<Button
								key="home"
								icon={<HomeOutlined />}
								onClick={this.handleGoHome}
							>
								На главную
							</Button>,
							<Button
								key="reset"
								onClick={this.handleReset}
							>
								Попробовать снова
							</Button>,
						]}
					>
						{isDev && (errorStack || componentStack) && (
							<Alert
								message="Детали ошибки (только в режиме разработки)"
								type="error"
								description={
									<div style={{ maxHeight: '400px', overflow: 'auto' }}>
										{errorStack && (
											<div style={{ marginBottom: '16px' }}>
												<strong>Stack trace:</strong>
												<pre style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
													{errorStack}
												</pre>
											</div>
										)}
										{componentStack && (
											<div>
												<strong>Component stack:</strong>
												<pre style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
													{componentStack}
												</pre>
											</div>
										)}
									</div>
								}
								style={{ marginTop: '24px', textAlign: 'left' }}
							/>
						)}
					</Result>
				</div>
			)
		}

		return this.props.children
	}
}

export default ErrorBoundary
