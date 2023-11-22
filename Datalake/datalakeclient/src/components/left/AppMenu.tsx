import { NavLink } from "react-router-dom";
import './AppMenu.css'
import axios from "axios"
import { API } from "../../router/api"
import { TagSource } from "../../@types/Source"
import { useState, useEffect } from "react"
import { useInterval } from "../../hooks/useInterval"
import { useUpdateContext } from "../../context/updateContext"
import { BlockType } from "../../@types/BlockType"
import { ManualId } from "../../@types/enums/CustomSourcesIdentity";
import router from "../../router/router";

export function AppMenu() {

	const { lastUpdate } = useUpdateContext()
	
	const [ sources, setSources ] = useState([] as TagSource[])
	const [ blocks, setBlocks ] = useState([] as BlockType[])

	function load() {
		axios.get(API.sources.list).then(res => setSources(res.data)).catch(() => router.navigate('/offline'))
		axios.get(API.blocks.list).then(res => setBlocks(res.data)).catch(() => router.navigate('/offline'))
	}

	useEffect(load, [lastUpdate])
	useInterval(load, 10000)

	return <div className="app-menu">

		<div className="app-menu-block">
			<NavLink to={'/'}>Монитор</NavLink>
			{/* <div className="app-menu-sub">
				<NavLink to={'/logs'}>События</NavLink>
				<NavLink to={'/settings'}>Администрирование</NavLink>
			</div> */}
		</div>

		<div className="app-menu-block">
			<NavLink to={'/tags'}>Теги</NavLink>
			<div className="app-menu-sub">
				<NavLink to={'/viewer'}>Архив</NavLink>
			</div>
		</div>

		<div className="app-menu-block">
			<NavLink to={'/sources'}>Источники</NavLink>
			<div className="app-menu-sub">
				{[
					...sources.map(x => <NavLink key={x.Id} to={`/sources/${x.Id}`}>{x.Name}</NavLink>)
					,<NavLink key={ManualId} to={'/tags/manual/'}>Мануальные теги</NavLink>
					/* ,<NavLink to={CalculatedId}>Вычисляемые теги</NavLink> */
				]}
			</div>
		</div>
		
		<div className="app-menu-block">
			<NavLink to={'/blocks'}>Объекты</NavLink>
			<div className="app-menu-sub">
				{blocks.map(x => <NavLink key={x.Id} to={`/blocks/${x.Id}`}>{x.Name}</NavLink>)}
			</div>
		</div>
	</div>
}