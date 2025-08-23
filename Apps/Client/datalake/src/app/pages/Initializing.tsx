import { Layout, Spin } from 'antd'
import { observer } from 'mobx-react-lite'

export const Initializing = observer(() => {
	return (
		<Layout>
			<Layout.Content style={{ height: '100vh', padding: '1em' }}>
				<Spin />
			</Layout.Content>
		</Layout>
	)
})
