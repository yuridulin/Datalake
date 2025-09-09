import { useAppStore } from '@/store/useAppStore'
import { Divider, Layout, theme } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { observer } from 'mobx-react-lite'
import { Outlet } from 'react-router-dom'
import RequireAuth from './auth/RequireAuth'
import AppMenu from './menu/AppMenu'
import AppMenuTopPanel from './menu/AppMenuTopPanel'

const AppLayout = observer(() => {
	const store = useAppStore()
	const { token } = theme.useToken()

	const siderStyle: React.CSSProperties = {
		backgroundColor: token.colorBgLayout,
		borderRight: '1px solid ' + token.colorSplit,
		paddingTop: '.5em',
		overflow: 'auto',
		height: '100vh',
		position: 'fixed',
		insetInlineStart: 0,
		top: 0,
		bottom: 0,
		scrollbarWidth: 'thin',
		scrollbarColor: 'unset',
		zIndex: 1,
	}
	return (
		<RequireAuth>
			{store.isDark && <style>{':root { color-scheme: dark; }'}</style>}
			{store.isAuthenticated && (
				<Layout
					hasSider
					style={{
						minWidth: '80em',
						height: '100vh',
					}}
				>
					<Sider width='20em' style={siderStyle}>
						<AppMenuTopPanel />
						<Divider variant='dotted' style={{ margin: '.5em 0' }} />
						<AppMenu />
					</Sider>
					<Layout
						style={{
							marginInlineStart: '20em',
						}}
					>
						<Content
							style={{
								overflow: 'auto',
								scrollbarWidth: 'thin',
								scrollbarColor: 'unset',
								padding: 24,
								backgroundColor: token.colorBgContainer,
							}}
						>
							<Outlet />
						</Content>
					</Layout>
				</Layout>
			)}
		</RequireAuth>
	)
})

export default AppLayout
