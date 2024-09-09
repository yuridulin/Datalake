import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { LogInfo } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import LogTypeEl from '../../components/LogTypeEl'
import routes from '../../router/routes'

export default function Dashboard() {
	const [logs, setLogs] = useState([] as LogInfo[])
	const navigate = useNavigate()

	const update = useCallback(() => {
		setLogs((prevLogs) => {
			const lastId = prevLogs.length > 0 ? prevLogs[0].id : 0
			api.configGetLogs({ lastId })
				.then((res) => {
					let newLogs = [...res.data, ...prevLogs]
					if (newLogs.length > 50) newLogs = newLogs.slice(-50)
					setLogs(newLogs)
				})
				.catch(() => navigate(routes.offline))
			return prevLogs
		})
	}, [navigate])

	useEffect(update, [update])
	useInterval(update, 5000)

	return (
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
