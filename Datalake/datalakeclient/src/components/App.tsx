import { useEffect, useState } from 'react'
import { Link, Outlet } from 'react-router-dom'
import { UpdateContext } from '../context/updateContext'
import { AppMenu } from './left/AppMenu'
import UserPanel from './global/UserPanel'
import { auth } from '../etc/auth'
import { ConfigProvider, theme } from 'antd'

export default function App() {

	const [ lastUpdate, setUpdate ] = useState<Date>(new Date())
	const [ checkedTags, setCheckedTags ] = useState<string[]>([])

	const { defaultAlgorithm, darkAlgorithm } = theme
	const [isDarkMode, setIsDarkMode] = useState(false)

	useEffect(() => {
		setIsDarkMode(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches)
		window.matchMedia('(prefers-color-scheme: dark)')
			.addEventListener('change', event => {
				setIsDarkMode(event.matches)
			})
	}, [])

	return (
		<ConfigProvider
			theme={{
				algorithm: isDarkMode ? darkAlgorithm : defaultAlgorithm,
			}}
		>
			<UpdateContext.Provider value={{ lastUpdate, setUpdate, checkedTags, setCheckedTags, }}>
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
			</UpdateContext.Provider>
		</ConfigProvider>
	)
}