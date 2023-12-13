import { Link, Outlet } from 'react-router-dom'
import { AppMenu } from './left/AppMenu'
import UserPanel from './global/UserPanel'
import { auth } from '../etc/auth'

export default function App() {

	return <>
		{!!auth.token && <>
			<div className="left">
				<Link to="/" className="title">Datalake</Link>
				<UserPanel />
				<AppMenu />
			</div>
			<div className="right">
				<Outlet />
			</div>
		</>}
	</>
}