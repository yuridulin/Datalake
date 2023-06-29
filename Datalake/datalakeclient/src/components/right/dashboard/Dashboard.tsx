import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import axios from "axios"
import { Navigate } from "react-router-dom"
import { useInterval } from "../../../hooks/useInterval"

type StatsType = {
	TotalTagsCount: number,
	TotalSourcesCount: number,
	WritesInMinute: number,
}

export default function Dashboard() {

	const [ stats, setStats ] = useState({} as StatsType)

	const [ update,, error ] = useFetching(async () => {
		let res = await axios.post('config/statistic')
		setStats(res.data)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { update() }, [])
	useInterval(update, 1000)

	return (
		error
			? <Navigate to="/offline" />
			: <>
				<div>
					Кол-во тегов: {stats.TotalTagsCount}
				</div>
				<div>
					Кол-во источников: {stats.TotalSourcesCount}
				</div>
				<div>
					Записей в минуту: {stats.WritesInMinute}
				</div>
			</>
	)
}