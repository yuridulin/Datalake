import api from '@/api/swagger-api'
import { LogInfo } from '@/api/swagger/data-contracts'
import UserButton from '@/app/components/buttons/UserButton'
import { useInterval } from '@/hooks/useInterval'
import { Button, Spin, Table } from 'antd'
import { ColumnType } from 'antd/es/table'
import { useEffect, useState } from 'react'

type LogsTableElProps = {
	sourceId?: number
	blockId?: number
	tagGuid?: string
	userGuid?: string
	userGroupGuid?: string
}

const columns: ColumnType<LogInfo>[] = [
	{
		dataIndex: 'dateString',
		title: 'Дата',
		width: '10em',
	},
	{
		title: 'Автор',
		dataIndex: 'author',
		width: '14em',
		render: (_, record: LogInfo) => (record.author ? <UserButton userInfo={record.author} /> : <></>),
	},
	{
		title: 'Событие',
		dataIndex: 'text',
		width: '16em',
	},
	{
		title: 'Описание',
		dataIndex: 'details',
	},
]

const LogsTableEl = ({ sourceId, blockId, tagGuid, userGuid, userGroupGuid }: LogsTableElProps) => {
	const [logs, setLogs] = useState([] as LogInfo[])
	const [loading, setLoading] = useState(false)
	const [reachEnd, setReachEnd] = useState(false)

	const initialLoad = () => {
		console.log('initialLoad')
		api
			.systemGetLogs({
				lastId: null,
				take: 10,
				source: sourceId,
				block: blockId,
				tag: tagGuid,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				setLogs(res.data)
				if (res.data.length < 10) setReachEnd(true)
			})
			.catch(() => setLogs([]))
	}

	const loadNewLogs = () => {
		const lastId = logs.reduce((acc, log) => (log.id > acc ? log.id : acc), 0)
		console.log('loadNewLogs: lastId = ' + String(lastId))
		api
			.systemGetLogs({
				lastId: lastId,
				source: sourceId,
				block: blockId,
				tag: tagGuid,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				setLogs([...res.data, ...logs])
			})
			.catch(() => {})
	}

	const loadOldLogs = () => {
		const firstId = logs.reduce((acc, log) => (log.id < acc ? log.id : acc), Infinity)
		setLoading(true)
		api
			.systemGetLogs({
				firstId: firstId,
				take: 10,
				source: sourceId,
				block: blockId,
				tag: tagGuid,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				setLogs([...logs, ...res.data])
				if (res.data.length < 10) setReachEnd(true)
			})
			.catch(() => setLogs([...logs]))
			.finally(() => setLoading(false))
	}

	useEffect(initialLoad, [sourceId, blockId, tagGuid, userGuid, userGroupGuid])
	useInterval(loadNewLogs, 10000)

	return (
		<>
			<Table size='small' columns={columns} dataSource={logs} rowKey={'id'} pagination={false} />
			{reachEnd || (
				<Button size='small' disabled={loading} icon={loading ? <Spin /> : <></>} onClick={loadOldLogs}>
					Загрузить предыдущие
				</Button>
			)}
		</>
	)
}

export default LogsTableEl
