import { Link, Outlet } from 'react-router-dom'
import { auth } from '../etc/auth'
import UserPanel from './global/UserPanel'
import { AppMenu } from './left/AppMenu'

export default function App() {
	return (
		<>
			{!!auth.token && (
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
