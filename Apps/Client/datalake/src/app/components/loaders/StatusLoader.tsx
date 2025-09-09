import { LoadStatus } from '@/app/components/loaders/loaderTypes'
import { theme } from 'antd'
import React, { useEffect, useRef } from 'react'

interface StatusLoaderProps {
	status: LoadStatus
	duration?: number
	after?: () => void
}

const StatusLoader: React.FC<StatusLoaderProps> = ({ status, duration = 400, after }) => {
	const { token } = theme.useToken()
	const timerRef = useRef<number>(0)

	// Автоматический сброс статусов success/error через указанное время
	useEffect(() => {
		if (status === 'success' || status === 'error') {
			timerRef.current = window.setTimeout(() => {
				if (after) after()
			}, duration)
		}

		return () => {
			if (timerRef.current) {
				clearTimeout(timerRef.current)
			}
		}
	}, [status, duration, after])

	// Эффект для добавления стилей бегунка
	useEffect(() => {
		const style = document.createElement('style')
		style.textContent = `
			@keyframes run {
				0% { transform: translateX(-100%); }
				100% { transform: translateX(500%); }
			}
			.loading-runner {
				position: absolute;
				top: 0;
				left: 0;
				height: 100%;
				width: 20%;
				background-color: ${token.colorWarning};
				animation: run 1s infinite linear;
				box-shadow: 0 0 5px rgba(255, 193, 7, 0.1);
				opacity: 0.3;
				z-index: 3;
			}
		`
		document.head.appendChild(style)

		return () => {
			document.head.removeChild(style)
		}
	}, [token.colorWarning])

	return (
		<div
			style={{
				position: 'relative',
				width: '100%',
				height: '2px',
				backgroundColor: 'rgba(0,0,0,0.1)',
				overflow: 'hidden',
			}}
		>
			{/* Полоса загрузки (желтая) */}
			{status === 'loading' && (
				<div
					style={{
						position: 'absolute',
						top: 0,
						left: 0,
						height: '100%',
						backgroundColor: token.colorWarning,
						width: '100%',
						opacity: 0.2,
						zIndex: 2,
					}}
				/>
			)}

			{/* Полоса успеха (зеленая) */}
			{status === 'success' && (
				<div
					style={{
						position: 'absolute',
						top: 0,
						left: 0,
						height: '100%',
						backgroundColor: token.colorSuccess,
						width: '100%',
						opacity: 0.3,
						transition: 'opacity 0.5s ease-out',
						zIndex: 2,
					}}
				/>
			)}

			{/* Полоса ошибки (красная) */}
			{status === 'error' && (
				<div
					style={{
						position: 'absolute',
						top: 0,
						left: 0,
						height: '100%',
						backgroundColor: token.colorError,
						width: '100%',
						opacity: 0.6,
						transition: 'opacity 0.5s ease-out',
						zIndex: 2,
					}}
				/>
			)}

			{/* Бегунок для состояния загрузки */}
			{status === 'loading' && <div className='loading-runner' />}
		</div>
	)
}

export default StatusLoader
