import { useState } from 'react'
import { Link, Outlet } from 'react-router-dom'
import { UpdateContext } from '../context/updateContext'
import { AppMenu } from './left/AppMenu'
import UserPanel from './global/UserPanel'

export default function App() {

	const [ lastUpdate, setUpdate ] = useState<Date>(new Date())
	const [ checkedTags, setCheckedTags ] = useState<string[]>([])

	return (
		<UpdateContext.Provider value={{ lastUpdate, setUpdate, checkedTags, setCheckedTags, }}>
			<div className="left">
				<Link to="/" className="title">Datalake</Link>
				<UserPanel />
				<AppMenu />
			</div>
			<div className="right">
				<Outlet />
			</div>
		</UpdateContext.Provider>
	)
}