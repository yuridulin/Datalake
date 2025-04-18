import api from '@/api/swagger-api'
import { LogCategory, LogInfo } from '@/api/swagger/data-contracts'
import UserButton from '@/app/components/buttons/UserButton'
import LogCategoryEl from '@/app/components/LogCategoryEl'
import getLogCategoryName from '@/functions/getLogCategoryName'
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
		dataIndex: 'category',
		title: 'Категория',
		width: '12em',
		render: (category) => <LogCategoryEl category={category} />,
		filters: Object.keys(LogCategory)
			.filter((x) => !isNaN(Number(x)))
			.map((x) => ({
				text: getLogCategoryName(Number(x)),
				value: Number(x),
			})),
		onFilter: (value, record) => record.category === value,
	},
	{
		title: 'Автор',
		dataIndex: 'author',
		width: '14em',
		render: (author) => (author ? <UserButton userInfo={author} /> : <i>нет</i>),
	},
	{
		title: 'Событие',
		dataIndex: 'text',
		width: '22em',
	},
	{
		title: 'Описание',
		dataIndex: 'details',
		render: (desc) => (
			<div style={{ wordBreak: 'break-all' }}>
				{desc.split('\n').map((x: string) => (
					<div>{x}</div>
				))}
			</div>
		),
	},
]

const step = 10
const globalStep = 100

const LogsTableEl = ({ sourceId, blockId, tagGuid, userGuid, userGroupGuid }: LogsTableElProps) => {
	const [logs, setLogs] = useState([] as LogInfo[])
	const [loading, setLoading] = useState(false)
	const [reachEnd, setReachEnd] = useState(false)
	const [isGlobal, setGlobal] = useState(false)

	const initialLoad = () => {
		console.log('initialLoad')
		const global = !sourceId && !blockId && !tagGuid && !userGuid && !userGroupGuid
		console.log('is global?', global)
		setGlobal(global)
		api
			.systemGetLogs({
				lastId: null,
				take: global ? globalStep : step,
				source: sourceId,
				block: blockId,
				tag: tagGuid,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				setLogs(res.data)
				if (res.data.length < (global ? globalStep : step)) setReachEnd(true)
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
				take: isGlobal ? globalStep : step,
				source: sourceId,
				block: blockId,
				tag: tagGuid,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				setLogs([...logs, ...res.data])
				if (res.data.length < (isGlobal ? globalStep : step)) setReachEnd(true)
			})
			.catch(() => setLogs([...logs]))
			.finally(() => setLoading(false))
	}

	useEffect(initialLoad, [sourceId, blockId, tagGuid, userGuid, userGroupGuid])
	useInterval(loadNewLogs, 10000)

	return (
		<>
			<Table
				size='small'
				columns={columns.filter((x) => isGlobal || x.dataIndex != 'category')}
				dataSource={logs}
				rowKey={'id'}
				pagination={false}
				scroll={isGlobal ? { y: 760 } : {}}
			/>
			<div style={{ margin: '.5em' }}>
				{reachEnd || (
					<Button size='small' disabled={loading} onClick={loadOldLogs}>
						Загрузить предыдущие
					</Button>
				)}
				&ensp;
				{loading && <Spin size='small' />}
			</div>
		</>
	)
}

export default LogsTableEl
