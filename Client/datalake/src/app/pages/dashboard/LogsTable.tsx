import { Button, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useCallback, useEffect, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import getLogCategoryName from '../../../api/models/getLogCategoryName'
import getLogTypeName from '../../../api/models/getLogTypeName'
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
import routes from '../../router/routes'

type FilterType = {
	categories: LogCategory[] | null
	types: LogType[] | null
}

export default function LogsTable() {
	const [logs, setLogs] = useState([] as LogInfo[])
	const [filter, setFilter] = useState({
		categories: [],
		types: [],
	} as FilterType)
	const navigate = useNavigate()
	const count = 15

	const update = useCallback(() => {
		setLogs((prevLogs) => {
			api.systemGetLogs({
				take: count,
				categories: filter.categories,
				types: filter.types,
			})
				.then((res) => {
					setLogs(res.data)
				})
				.catch(() => navigate(routes.offline))
			return prevLogs
		})
	}, [navigate, filter])

	useEffect(update, [update, filter])
	useInterval(update, 5000)

	return (
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
