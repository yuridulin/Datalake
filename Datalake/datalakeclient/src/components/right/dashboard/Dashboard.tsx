import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import axios from "axios"
import { Navigate } from "react-router-dom"
import { useInterval } from "../../../hooks/useInterval"
import { Log } from "../../../@types/Log"
import ProgramLogType from "../../small/ProgramLogType"

type StatsType = {
	TotalTagsCount: number,
	TotalSourcesCount: number,
	WritesInMinute: number,
	Logs: Log[],
	Last: Date
}

export default function Dashboard() {

	const [ stats, setStats ] = useState({ Logs: [] as Log[], Last: new Date() } as StatsType)

	const [ update,, error ] = useFetching(async () => {
		let res = await axios.post('config/statistic', { Last: stats.Last })
		let newStats = res.data as StatsType
		let logs =  [ ...stats.Logs, ...newStats.Logs ]
		if (logs.length > 50) logs = logs.slice(-50)
		setStats({ 
			TotalTagsCount: newStats.TotalTagsCount,
			TotalSourcesCount: newStats.TotalSourcesCount,
			WritesInMinute: newStats.WritesInMinute,
			Logs: logs,
			Last: newStats.Last,
		})
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { console.log(stats); update() }, [])
	useInterval(update, 5000)

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

				<div className="table">
					<div className="table-header">
						<span>Время</span>
						<span>Модуль</span>
						<span>Сообщение</span>
						<span>Тип</span>
					</div>
					{stats.Logs.map((x, i) => <div className="table-row" key={i}>
						<span>{x.Date.toString()}</span>
						<span>{x.Module}</span>
						<span>{x.Message}</span>
						<span><ProgramLogType type={x.Type} /></span>
					</div>)}
				</div>
			</>
	)
}