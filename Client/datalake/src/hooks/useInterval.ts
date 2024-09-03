import { useEffect, useRef } from 'react'

export const useInterval = (callback: never | (() => void), delay: number) => {
	const savedCallback = useRef()

	useEffect(() => {
		savedCallback.current = callback as never
	}, [callback])

	useEffect(() => {
		function tick() {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			;(savedCallback as any).current()
		}
		if (delay !== null) {
			const id = setInterval(tick, delay)
			return () => clearInterval(id)
		}
	}, [delay])
}
