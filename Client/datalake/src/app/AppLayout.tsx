import { Layout } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { Link, Outlet } from 'react-router-dom'
import { isAuth } from '../api/local-auth'
import { useUpdateContext } from '../context/updateContext'
import { AppMenu } from './components/AppMenu'
import UserPanel from './components/UserPanel'

export default function AppLayout() {
	const { isDarkMode } = useUpdateContext()

	const siderStyle: React.CSSProperties = {
		backgroundColor: isDarkMode ? '#141414' : '#eee',
		borderRight: '1px solid ' + (isDarkMode ? '#222' : '#ddd'),
		paddingTop: '1em',
		overflow: 'auto',
		height: '100vh',
		position: 'fixed',
		insetInlineStart: 0,
		top: 0,
		bottom: 0,
		scrollbarWidth: 'thin',
		scrollbarColor: 'unset',
	}
	return (
		<>
			{isAuth() && (
				<Layout hasSider>
					<Sider width='20%' style={siderStyle}>
						<Link
							to='/'
							className='title'
							style={{
								padding: '1.5em 1em',
							}}
						>
							Datalake
						</Link>
						<UserPanel />
						<AppMenu />
					</Sider>
					<Layout
						style={{
							marginInlineStart: '20%',
							backgroundColor: isDarkMode ? '#121212' : '#fff',
						}}
					>
						<Content
							style={{
								overflow: 'initial',
								scrollbarWidth: 'thin',
								scrollbarColor: 'unset',
							}}
						>
							<div
								style={{
									padding: 24,
								}}
							>
								<Outlet />
							</div>
						</Content>
					</Layout>
				</Layout>
			)}
		</>
	)
}
