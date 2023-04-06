import { useEffect } from "react"
import { Navigate, NavLink } from "react-router-dom"
import appApi from "../../api/appApi"
import { useUpdateContext } from "../../context/updateContext"
import { useFetching } from "../../hooks/useFetching"
import { useInterval } from "../../hooks/useInterval"

export default function Tree() {

	const { lastUpdate } = useUpdateContext()

	const [ load, , error ] = useFetching(async () => {
		await appApi.lastUpdate()
	})

	// eslint-disable-next-line
	useEffect(() => { load() }, [lastUpdate])
	useInterval(load, 10000)

	return (
		error
			? <Navigate to="/offline" />
			: <div className="tree">
				<div className="tree-item">
					<i className="material-icons">pin</i>
					<NavLink to="/values">Значения</NavLink>
				</div>
				<div className="tree-item">
					<i className="material-icons">style</i>
					<NavLink to="/tags">Теги</NavLink>
				</div>
				<div className="tree-item">
					<i className="material-icons">mediation</i>
					<NavLink to="/sources">Источники</NavLink>
				</div>
			</div>
	)
}