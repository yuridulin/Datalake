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
	const pollingFunctionRef = useRef(pollingFunction) // Ref для актуальной функции
	const hasStartedPollingRef = useRef(false) // Флаг для отслеживания запуска polling

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

	useEffect(() => console.log('polling status', [status]), [status])

	// Обновляем ref при изменении pollingFunction
	useEffect(() => {
		pollingFunctionRef.current = pollingFunction
	}, [pollingFunction])

	const executePollRef = useRef<(() => Promise<void>) | undefined>(undefined)

	const executePoll = useCallback(async () => {
		if (!isMountedRef.current) {
			console.log('executePoll skipped - component unmounted')
			return // Прерываем если компонент размонтирован
		}

		console.log('executePoll called')
		setStatus('loading')

		try {
			await pollingFunctionRef.current()
			if (!isMountedRef.current) {
				console.log('executePoll skipped - component unmounted after pollingFunction')
				return // Проверяем после асинхронной операции
			}

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
						executePollRef.current?.()
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
						executePollRef.current?.()
					}
				}

				animationRef.current = requestAnimationFrame(animateWaiting)
			}, statusDuration)
		}
	}, [interval, statusDuration])

	// Обновляем ref при изменении executePoll
	executePollRef.current = executePoll

	useEffect(() => {
		console.log(
			'PollingLoader useEffect triggered, isInitialMount:',
			isInitialMount.current,
			'hasStartedPolling:',
			hasStartedPollingRef.current,
		)
		isMountedRef.current = true // Устанавливаем флаг при монтировании

		// Очищаем предыдущие таймеры и анимации перед запуском новых
		if (pollingRef.current) {
			clearTimeout(pollingRef.current)
			pollingRef.current = 0
		}
		if (animationRef.current) {
			cancelAnimationFrame(animationRef.current)
			animationRef.current = 0
		}

		// Предотвращаем двойной запуск при первом монтировании (StrictMode)
		if (isInitialMount.current) {
			if (!hasStartedPollingRef.current) {
				hasStartedPollingRef.current = true
				console.log('Initial mount - calling executePoll')
				// Не сбрасываем isInitialMount здесь, чтобы предотвратить запуск анимации при повторном вызове эффекта в StrictMode
				executePollRef.current?.()
			} else {
				console.log('Initial mount already processed, skipping')
			}
			return
		}

		// Если это не первый монтирование и polling уже запущен, не запускаем анимацию заново
		// (это может произойти при изменении interval)
		if (hasStartedPollingRef.current) {
			console.log('Polling already started, skipping animation setup')
			return
		}

		console.log('Not initial mount - starting animation')
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
				console.log('Animation complete - calling executePoll')
				executePollRef.current?.()
			}
		}

		animationRef.current = requestAnimationFrame(animateWaiting)

		return () => {
			console.log('PollingLoader cleanup')
			isMountedRef.current = false // Сбрасываем флаг при размонтировании
			hasStartedPollingRef.current = false // Сбрасываем флаг при размонтировании
			isInitialMount.current = true // Сбрасываем флаг для следующего монтирования
			if (pollingRef.current) {
				clearTimeout(pollingRef.current)
				pollingRef.current = 0
			}
			if (animationRef.current) {
				cancelAnimationFrame(animationRef.current)
				animationRef.current = 0
			}
		}
	}, [interval])

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
