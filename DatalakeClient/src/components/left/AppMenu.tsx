import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../api/swagger-api'
import { BlockTreeInfo, SourceInfo } from '../../api/swagger/data-contracts'
import { useUpdateContext } from '../../context/updateContext'
import { auth } from '../../etc/auth'
import { CustomSource } from '../../etc/customSource'

export function AppMenu() {
	const { lastUpdate } = useUpdateContext()

	const [sources, setSources] = useState([] as SourceInfo[])
	const [blocks, setBlocks] = useState([] as BlockTreeInfo[])

	function load() {
		api.blocksReadAsTree().then((res) => setBlocks(res.data))
		api.sourcesReadAll().then((res) => setSources(res.data))
	}

	useEffect(load, [lastUpdate])
	//useInterval(load, 10000)

	return (
		<div className='app-menu'>
			{auth.isAdmin() && <NavLink to={'/users'}>Пользователи</NavLink>}

			<div className='app-menu-block'>
				<NavLink to={'/'}>Монитор</NavLink>
				{/* <div className="app-menu-sub">
				<NavLink to={'/logs'}>События</NavLink>
				<NavLink to={'/settings'}>Администрирование</NavLink>
			</div> */}
			</div>

			<div className='app-menu-block'>
				<NavLink to={'/tags'}>Теги</NavLink>
				<div className='app-menu-sub'>
					<NavLink to={'/viewer'}>Архив</NavLink>
				</div>
			</div>

			<div className='app-menu-block'>
				<NavLink to={'/sources'}>Источники</NavLink>
				<div className='app-menu-sub'>
					{sources
						.filter((x) => x.id > 0)
						.map((x) => (
							<NavLink key={x.id} to={`/sources/${x.id}`}>
								{x.name}
							</NavLink>
						))}
				</div>
			</div>

			<div className='app-menu-block'>
				<NavLink key={CustomSource.Manual} to={'/tags/manual/'}>
					Мануальные теги
				</NavLink>
				<NavLink key={CustomSource.Calculated} to={'/tags/calc/'}>
					Вычисляемые теги
				</NavLink>
			</div>

			<div className='app-menu-block'>
				<NavLink to={'/blocks'}>Объекты</NavLink>
				<div className='app-menu-sub'>
					{blocks.map((x) => (
						<NavLink key={x.id} to={`/blocks/${x.id}`}>
							{x.name}
						</NavLink>
					))}
				</div>
			</div>
		</div>
	)
}
