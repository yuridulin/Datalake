import { Divider, Layout, notification, theme } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { motion } from 'framer-motion'
import { Outlet, useLocation } from 'react-router-dom'
import { isAuth } from '../api/local-auth'
import { AppMenu } from './components/AppMenu'
import LogoPanel from './components/LogoPanel'
import UserPanel from './components/UserPanel'

notification.config({
	placement: 'bottomLeft',
	bottom: 50,
	duration: 5,
})

export default function AppLayout() {
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
			{isAuth() && (
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
}
