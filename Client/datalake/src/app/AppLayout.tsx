import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Layout, notification, theme } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { motion } from 'framer-motion'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { isAuth } from '../api/local-auth'
import { useUpdateContext } from '../context/updateContext'
import { AppMenu } from './components/AppMenu'
import UserPanel from './components/UserPanel'

notification.config({
	placement: 'bottomLeft',
	bottom: 50,
	duration: 5,
})

export default function AppLayout() {
	const { isDarkMode, setDarkMode } = useUpdateContext()
	const { token } = theme.useToken()
	const { pathname } = useLocation()

	const siderStyle: React.CSSProperties = {
		backgroundColor: token.colorBgLayout,
		borderRight: '1px solid ' + token.colorSplit,
		paddingTop: '1em',
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
						<Link
							to='/'
							className='title'
							style={{
								padding: '1.5em 1em',
							}}
						>
							Datalake
						</Link>
						{isDarkMode ? (
							<MoonOutlined
								style={{ color: '#ccc' }}
								onClick={() => setDarkMode(false)}
							/>
						) : (
							<SunOutlined
								style={{ color: '#666' }}
								onClick={() => setDarkMode(true)}
							/>
						)}
						<UserPanel />
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
