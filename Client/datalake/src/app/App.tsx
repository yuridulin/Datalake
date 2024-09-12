import { Layout } from 'antd'
import Sider from 'antd/es/layout/Sider'
import { Content } from 'antd/es/layout/layout'
import { Link, Outlet } from 'react-router-dom'
import { isAuth } from '../api/local-auth'
import { useUpdateContext } from '../context/updateContext'
import { AppMenu } from './components/AppMenu'
import UserPanel from './components/UserPanel'

export default function App() {
	const { isDarkMode } = useUpdateContext()
	const layoutStyle = {
		minHeight: '100vh',
		//borderRadius: 8,
		//overflow: 'hidden',
		//width: 'calc(50% - 8px)',
		//maxWidth: 'calc(50% - 8px)',
	}
	const siderStyle: React.CSSProperties = {
		//textAlign: 'center',
		//lineHeight: '120px',
		backgroundColor: isDarkMode ? '#141414' : '#eee',
		paddingTop: '1em',
	}
	const contentStyle: React.CSSProperties = {
		padding: '20px',
	}
	return (
		<>
			{isAuth() && (
				<Layout style={layoutStyle}>
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
					<Layout>
						<Content style={contentStyle}>
							<Outlet />
						</Content>
					</Layout>
				</Layout>
			)}
		</>
	)
}
