import PollingLoader from '@/app/components/loaders/PollingLoader'
import { useAppStore } from '@/store/useAppStore'
import { Layout } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useRef } from 'react'

export const Offline = observer(() => {
	const { refreshAuthData } = useAppStore()
	const isMountedRef = useRef(true)

	// Отслеживаем монтирование компонента
	useEffect(() => {
		isMountedRef.current = true
		return () => {
			isMountedRef.current = false
		}
	}, [])

	const pollingFunc = () => {
		if (!isMountedRef.current) return
		refreshAuthData()
	}

	return (
		<Layout>
			<Layout.Content style={{ height: '100vh', padding: '1em' }}>
				<PollingLoader pollingFunction={pollingFunc} interval={2000} />
				<div>Нет соединения</div>
			</Layout.Content>
		</Layout>
	)
})
