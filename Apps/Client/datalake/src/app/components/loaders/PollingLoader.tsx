import { theme } from 'antd'
import dayjs from 'dayjs'
import React, { useCallback, useEffect, useRef, useState } from 'react'

interface PollingLoaderProps {
	pollingFunction: (() => Promise<never>) | (() => void)
	interval: number
	statusDuration?: number
}

const PollingLoader: React.FC<PollingLoaderProps> = ({ pollingFunction, interval, statusDuration = 400 }) => {
	const { token } = theme.useToken()
	const [status, setStatus] = useState<'waiting' | 'loading' | 'success' | 'error' | 'default'>('default')
	const [progress, setProgress] = useState(0)
	const pollingRef = useRef<number>(0)
	const animationRef = useRef<number>(0)
	const startTimeRef = useRef<dayjs.Dayjs>(dayjs())
	const lastUpdateRef = useRef<dayjs.Dayjs>(dayjs())
	const isInitialMount = useRef(true)
	const isMountedRef = useRef(true) // Добавляем флаг монтирования

	// Добавляем стили для анимации бегунка
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

	const executePoll = useCallback(async () => {
		if (!isMountedRef.current) return // Прерываем если компонент размонтирован

		setStatus('loading')

		try {
			await pollingFunction()
			if (!isMountedRef.current) return // Проверяем после асинхронной операции

			setStatus('success')

			pollingRef.current = window.setTimeout(() => {
				if (!isMountedRef.current) return
				setStatus('waiting')
				setProgress(0)
				startTimeRef.current = dayjs()
				lastUpdateRef.current = dayjs()

				const animateWaiting = () => {
					if (!isMountedRef.current) return
					const now = dayjs()
					const elapsed = now.diff(startTimeRef.current)
					const newProgress = Math.min((elapsed / interval) * 100, 100)

					setProgress(newProgress)
					lastUpdateRef.current = now

					if (newProgress < 100) {
						animationRef.current = requestAnimationFrame(animateWaiting)
					} else {
						executePoll()
					}
				}

				animationRef.current = requestAnimationFrame(animateWaiting)
			}, statusDuration)
		} catch {
			if (!isMountedRef.current) return
			setStatus('error')

			pollingRef.current = window.setTimeout(() => {
				if (!isMountedRef.current) return
				setStatus('waiting')
				setProgress(0)
				startTimeRef.current = dayjs()
				lastUpdateRef.current = dayjs()

				const animateWaiting = () => {
					if (!isMountedRef.current) return
					const now = dayjs()
					const elapsed = now.diff(startTimeRef.current)
					const newProgress = Math.min((elapsed / interval) * 100, 100)

					setProgress(newProgress)
					lastUpdateRef.current = now

					if (newProgress < 100) {
						animationRef.current = requestAnimationFrame(animateWaiting)
					} else {
						executePoll()
					}
				}

				animationRef.current = requestAnimationFrame(animateWaiting)
			}, statusDuration)
		}
	}, [pollingFunction, interval, statusDuration])

	useEffect(() => {
		isMountedRef.current = true // Устанавливаем флаг при монтировании

		if (isInitialMount.current) {
			isInitialMount.current = false
			executePoll()
			return
		}

		setStatus('waiting')
		setProgress(0)
		startTimeRef.current = dayjs()
		lastUpdateRef.current = dayjs()

		const animateWaiting = () => {
			if (!isMountedRef.current) return // Прерываем если компонент размонтирован
			const now = dayjs()
			const elapsed = now.diff(startTimeRef.current)
			const newProgress = Math.min((elapsed / interval) * 100, 100)

			setProgress(newProgress)
			lastUpdateRef.current = now

			if (newProgress < 100) {
				animationRef.current = requestAnimationFrame(animateWaiting)
			} else {
				executePoll()
			}
		}

		animationRef.current = requestAnimationFrame(animateWaiting)

		return () => {
			isMountedRef.current = false // Сбрасываем флаг при размонтировании
			if (pollingRef.current) {
				clearTimeout(pollingRef.current)
			}
			if (animationRef.current) {
				cancelAnimationFrame(animationRef.current)
			}
		}
	}, [executePoll, interval])

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
			{/* Остальная разметка без изменений */}
			<div
				style={{
					position: 'absolute',
					top: 0,
					left: 0,
					height: '100%',
					backgroundColor: token.colorPrimary,
					width: `${progress}%`,
					transition: status === 'waiting' ? 'width 0.1s linear' : 'none',
					opacity: status === 'waiting' ? 0.3 : 0,
					zIndex: 1,
				}}
			/>

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

			{status === 'loading' && <div className='loading-runner' />}
		</div>
	)
}

export default PollingLoader
