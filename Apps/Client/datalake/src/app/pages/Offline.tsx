import { useAppStore } from '@/store/useAppStore'
import { Layout } from 'antd'
import { observer } from 'mobx-react-lite'
import { useInterval } from 'react-use'

export const Offline = observer(() => {
	const { refreshAuthData } = useAppStore()

	useInterval(refreshAuthData, 2000)

	return (
		<Layout>
			<Layout.Content style={{ height: '100vh', padding: '1em' }}>
				<div>Нет соединения</div>
			</Layout.Content>
		</Layout>
	)
})
