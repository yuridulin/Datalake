import { Button, Spin, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { AxiosResponse } from 'axios'
import { useCallback, useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import getLogCategoryName from '../../../api/functions/getLogCategoryName'
import getLogTypeName from '../../../api/functions/getLogTypeName'
import api from '../../../api/swagger-api'
import {
	LogCategory,
	LogInfo,
	LogType,
	UserSimpleInfo,
} from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import LogCategoryEl from '../../components/LogCategoryEl'
import LogTypeEl from '../../components/LogTypeEl'
import NoAccess from '../../components/NoAccessPage'
import routes from '../../router/routes'

type FilterType = {
	categories: LogCategory[] | null
	types: LogType[] | null
}

export default function LogsTable() {
	const [loading, setLoading] = useState(true)
	const [noAccess, setAccess] = useState(false)
	const [logs, setLogs] = useState([] as LogInfo[])
	const [filter, setFilter] = useState({
		categories: [],
		types: [],
	} as FilterType)
	const count = 15

	const update = useCallback(() => {
		if (noAccess) return
		setLogs((prevLogs) => {
			api.systemGetLogs({
				take: count,
				'categories[]': filter.categories,
				'types[]': filter.types,
			})
				.then((res) => {
					setAccess(false)
					setLogs(res.data)
				})
				.catch((res: AxiosResponse) => {
					if (res.status === 403) setAccess(true)
					setLogs([])
				})
				.finally(() => setLoading(false))
			return prevLogs
		})
	}, [filter, noAccess])

	useEffect(update, [update, filter])
	useInterval(update, 5000)

	return loading ? (
		<Spin />
	) : noAccess ? (
		<NoAccess />
	) : (
		<>
			<Table
				dataSource={logs}
				size='small'
				pagination={false}
				showSorterTooltip={false}
				onChange={(_, f) => {
					setFilter({
						categories: (f['category'] || []) as LogCategory[],
						types: (f['type'] || []) as LogType[],
					})
				}}
				rowKey='id'
			>
				<Column
					dataIndex='dateString'
					title='Дата'
					width='12em'
					sorter={(a: LogInfo, b: LogInfo) => (a.id > b.id ? 1 : -1)}
					defaultSortOrder='descend'
				/>
				<Column
					dataIndex='category'
					title='Категория'
					width='12em'
					render={(x: LogCategory) => <LogCategoryEl category={x} />}
					filters={Object.keys(LogCategory)
						.filter((x) => !isNaN(Number(x)))
						.map((x) => ({
							text: getLogCategoryName(Number(x)),
							value: x,
						}))}
				/>
				<Column
					dataIndex='type'
					title='Уровень'
					width='12em'
					render={(x: LogType) => <LogTypeEl type={x} />}
					filters={Object.keys(LogType)
						.filter((x) => !isNaN(Number(x)))
						.map((x) => ({
							text: getLogTypeName(Number(x)),
							value: x,
						}))}
				/>
				<Column dataIndex='text' title='Сообщение' />
				<Column<LogInfo>
					dataIndex='author'
					title='Пользователь'
					render={(author: UserSimpleInfo | undefined) => {
						if (!author) return <></>
						return (
							<NavLink to={routes.users.toUserForm(author.guid)}>
								<Button size='small'>{author.fullName}</Button>
							</NavLink>
						)
					}}
				/>
			</Table>
		</>
	)
}
