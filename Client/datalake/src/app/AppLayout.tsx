import { Divider, Layout, theme } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { motion } from 'framer-motion'
import { observer } from 'mobx-react-lite'
import { Outlet, useLocation } from 'react-router-dom'
import { user } from '../api/user'
import { AppMenu } from './AppMenu'
import LogoPanel from './components/LogoPanel'
import UserPanel from './components/UserPanel'

const AppLayout = observer(() => {
	const { token } = theme.useToken()
	const { pathname } = useLocation()

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
		<>
			{user.isDark() && <style>{':root { color-scheme: dark; }'}</style>}
			{user.isAuth() && (
				<Layout
					hasSider
					style={{
						minWidth: '80em',
						height: '100vh',
					}}
				>
					<Sider width='20em' style={siderStyle}>
						<LogoPanel />
						<UserPanel />
						<Divider
							variant='dotted'
							style={{ margin: '.5em 0' }}
						/>
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
							<motion.div
								layout
								key={pathname}
								initial='initial'
								animate='in'
							>
								<Outlet />
							</motion.div>
						</Content>
					</Layout>
				</Layout>
			)}
		</>
	)
})

export default AppLayout
