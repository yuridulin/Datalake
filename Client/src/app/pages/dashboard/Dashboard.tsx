import { useEffect, useState } from 'react'
import { Navigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { LogInfo } from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import LogTypeEl from '../../components/LogTypeEl'

export default function Dashboard() {
	const [logs, setLogs] = useState([] as LogInfo[])

	const [update, , error] = useFetching(async () => {
		api.configGetLogs().then((res) => {
			let newLogs = [...logs, ...res.data]
			if (newLogs.length > 50) newLogs = newLogs.slice(-50)
			setLogs(newLogs)
		})
	})

	useEffect(() => {
		update()
		let interval = setInterval(update, 5000)
		return () => clearInterval(interval)
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])

	return error ? (
		<Navigate to='/offline' />
	) : (
		<>
			{
				<div className='table'>
					<div className='table-header'>
						<span>Время</span>
						<span>Категория</span>
						<span>Сообщение</span>
						<span>Тип</span>
					</div>
					{logs.map((x, i) => (
						<div className='table-row' key={i}>
							<span>{x.dateString}</span>
							<span>{x.category}</span>
							<span>{x.text}</span>
							<span>
								<LogTypeEl type={x.type} />
							</span>
						</div>
					))}
				</div>
			}
		</>
	)
}
