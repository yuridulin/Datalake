import { Layout, Spin } from 'antd'

/**
 * Компонент fallback для Suspense при lazy loading страниц
 * Показывает спиннер загрузки, соответствующий дизайну приложения
 */
const LoadingFallback = () => {
	return (
		<Layout style={{ minHeight: '100vh' }}>
			<Layout.Content
				style={{
					display: 'flex',
					alignItems: 'center',
					justifyContent: 'center',
					padding: '24px',
				}}
			>
				<Spin size="large" />
			</Layout.Content>
		</Layout>
	)
}

export default LoadingFallback
