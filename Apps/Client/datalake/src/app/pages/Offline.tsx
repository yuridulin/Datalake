import PollingLoader from '@/app/components/loaders/PollingLoader'
import { useAppStore } from '@/store/useAppStore'
import { Layout } from 'antd'
import { observer } from 'mobx-react-lite'

export const Offline = observer(() => {
	const { refreshAuthData } = useAppStore()

	return (
		<Layout>
			<Layout.Content style={{ height: '100vh', padding: '1em' }}>
				<PollingLoader pollingFunction={refreshAuthData} interval={2000} />
				<div>Нет соединения</div>
			</Layout.Content>
		</Layout>
	)
})
