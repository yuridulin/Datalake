import { Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	LogCategory,
	LogInfo,
	LogType,
} from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import LogCategoryEl from '../../components/LogCategoryEl'
import LogTypeEl from '../../components/LogTypeEl'
import routes from '../../router/routes'

export default function LogsTable() {
	const [logs, setLogs] = useState([] as LogInfo[])
	const navigate = useNavigate()
	const count = 15

	const update = useCallback(() => {
		setLogs((prevLogs) => {
			const lastId = prevLogs.length > 0 ? prevLogs[0].id : 0
			api.configGetLogs({ lastId, take: count })
				.then((res) => {
					let newLogs = [...res.data, ...prevLogs]
					if (newLogs.length > count) newLogs = newLogs.slice(-count)
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
			<Table
				dataSource={logs}
				size='small'
				pagination={false}
				showSorterTooltip={false}
				rowKey='id'
			>
				<Column
					dataIndex='dateString'
					title='Дата'
					sorter={(a: LogInfo, b: LogInfo) => (a.id > b.id ? 1 : -1)}
					defaultSortOrder='descend'
				/>
				<Column
					dataIndex='category'
					title='Категория'
					render={(x: LogCategory) => <LogCategoryEl category={x} />}
				/>
				<Column dataIndex='text' title='Сообщение' />
				<Column
					dataIndex='type'
					title='Уровень'
					render={(x: LogType) => <LogTypeEl type={x} />}
				/>
			</Table>
		</>
	)
}
