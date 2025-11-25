import UserButton from '@/app/components/buttons/UserButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import LogCategoryEl from '@/app/components/LogCategoryEl'
import getLogCategoryName from '@/functions/getLogCategoryName'
import { LogCategory, LogInfo } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Button, Table } from 'antd'
import { ColumnType } from 'antd/es/table'
import { useMemo, useState } from 'react'

type LogsTableElProps = {
	sourceId?: number
	blockId?: number
	tagId?: number
	userGuid?: string
	userGroupGuid?: string
}

const columns: ColumnType<LogInfo>[] = [
	{
		key: 'id',
		dataIndex: 'id',
		title: 'Id',
		width: '3em',
	},
	{
		key: 'dateString',
		dataIndex: 'dateString',
		title: 'Дата',
		width: '10em',
	},
	{
		key: 'category',
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
		key: 'author',
		title: 'Автор',
		dataIndex: 'author',
		width: '14em',
		render: (author) => (author ? <UserButton userInfo={author} /> : <i>нет</i>),
	},
	{
		key: 'text',
		title: 'Событие',
		dataIndex: 'text',
		width: '22em',
	},
	{
		key: 'details',
		title: 'Описание',
		dataIndex: 'details',
		render: (desc) =>
			desc ? (
				<div style={{ wordBreak: 'break-all' }}>
					{desc.split('\n').map((x: string, i: number) => (
						<div key={i}>{x}</div>
					))}
				</div>
			) : (
				<></>
			),
	},
]

const LogsTableEl = ({ sourceId, blockId, tagId, userGuid, userGroupGuid }: LogsTableElProps) => {
	const store = useAppStore()

	const [logs, setLogs] = useState([] as LogInfo[])
	const [reachEnd, setReachEnd] = useState(false)
	const isGlobal = useMemo(
		() => !sourceId && !blockId && !tagId && !userGuid && !userGroupGuid,
		[sourceId, blockId, tagId, userGuid, userGroupGuid],
	)
	const step = useMemo(() => (isGlobal ? 100 : 10), [isGlobal])

	const addLogs = (newLogs: LogInfo[]) => {
		if (!newLogs.length) return
		logger.debug('addLogs', {
			component: 'LogsTableEl',
			action: 'addLogs',
			from: Math.min(...newLogs.map((x) => x.id)),
			to: Math.max(...newLogs.map((x) => x.id)),
		})
		setLogs(() => {
			const uniqueLogs: Record<number, LogInfo> = {}
			logs.concat(newLogs).forEach((log) => {
				uniqueLogs[log.id] = log
			})
			return Object.values(uniqueLogs).reverse()
		})
	}

	const loadNewLogs = () => {
		store.api
			.inventoryAuditGet({
				lastId: logs.reduce((acc, log) => (log.id > acc ? log.id : acc), 0),
				take: step,
				source: sourceId,
				block: blockId,
				tag: tagId,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				addLogs(res.data)
			})
			.catch(() => {})
	}

	const loadOldLogs = () => {
		store.api
			.inventoryAuditGet({
				firstId: logs.reduce((acc, log) => (log.id < acc ? log.id : acc), Infinity),
				take: step,
				source: sourceId,
				block: blockId,
				tag: tagId,
				user: userGuid,
				group: userGroupGuid,
			})
			.then((res) => {
				addLogs(res.data)
				// если пришло меньше, чем запросили - логов для загрузки не осталось, скрываем кнопку
				if (res.data.length < step) setReachEnd(true)
			})
			.catch(() => {})
	}

	return (
		<>
			<PollingLoader pollingFunction={loadNewLogs} interval={10000} />
			<Table
				rowKey='id'
				size='small'
				columns={columns.filter((x) => isGlobal || x.dataIndex != 'category')}
				dataSource={logs}
				pagination={false}
				scroll={isGlobal ? { y: 760 } : {}}
			/>
			<div style={{ margin: '.5em' }}>
				{reachEnd || (
					<Button size='small' onClick={loadOldLogs}>
						Загрузить предыдущие
					</Button>
				)}
			</div>
		</>
	)
}

export default LogsTableEl
