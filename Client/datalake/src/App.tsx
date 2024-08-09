import { Link, Outlet } from 'react-router-dom'
import { isAuth } from './api/auth'
import { AppMenu } from './app/components/AppMenu'
import UserPanel from './app/components/UserPanel'

export default function App() {
	return (
		<>
			{isAuth() && (
				<>
					<div className='left'>
						<Link to='/' className='title'>
							Datalake
						</Link>
						<UserPanel />
						<AppMenu />
					</div>
					<div className='right'>
						<Outlet />
					</div>
				</>
			)}
		</>
	)
}
