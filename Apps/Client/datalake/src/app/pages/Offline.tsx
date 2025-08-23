import { useAppStore } from '@/store/useAppStore'
import { Layout } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect } from 'react'

export const Offline = observer(() => {
	const { refreshAuthData } = useAppStore()

	useEffect(function () {
		const awaiter = setInterval(refreshAuthData, 2000)
		return () => clearInterval(awaiter)
	}, [])

	return (
		<Layout>
			<Layout.Content style={{ height: '100vh' }}>
				<div style={{ padding: '1em' }}>Нет соединения</div>
			</Layout.Content>
		</Layout>
	)
})
