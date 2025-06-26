import api from '@/api/swagger-api'
import { AccessType, SourceInfo, SourceState, SourceType } from '@/api/swagger/data-contracts'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import getSourceTypeName from '@/functions/getSourceTypeName'
import { useInterval } from '@/hooks/useInterval'
import { timeAgo } from '@/state/timeAgoInstance'
import { user } from '@/state/user'
import { CheckOutlined, DisconnectOutlined } from '@ant-design/icons'
import { Button, notification, Table, TableColumnsType, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'

interface DataCell extends SourceInfo {
	isGroup: boolean
	children?: DataCell[]
	link: string
}

const ExcludedTypes = [SourceType.NotSet, SourceType.System]
const NotEnteredTypes = [SourceType.Aggregated, SourceType.Calculated, SourceType.Manual]

const SourcesList = observer(() => {
	const [sources, setSources] = useState([] as DataCell[])
	const [states, setStates] = useState({} as Record<string, SourceState>)

	const load = () => {
		setSources([])
		api
			.sourcesReadAll({ withCustom: true })
			.then((res) => {
				const [system, user]: DataCell[] = [
					{
						isGroup: true,
						id: -2000,
						name: 'Системные источники',
						isDisabled: false,
						type: SourceType.NotSet,
						children: [],
						link: '',
					},
					{
						isGroup: true,
						id: -1000,
						name: 'Пользовательские источники',
						isDisabled: false,
						type: SourceType.NotSet,
						children: [],
						link: '',
					},
				]
				res.data.reduce((agg, next) => {
					if (ExcludedTypes.includes(next.type)) return agg
					if (NotEnteredTypes.includes(next.type)) {
						const link =
							next.type === SourceType.Aggregated
								? routes.tags.aggregated
								: next.type === SourceType.Calculated
									? routes.tags.calc
									: next.type === SourceType.Manual
										? routes.tags.manual
										: ''
						system.children?.push({ ...next, isGroup: false, link })
					} else {
						user.children?.push({ ...next, isGroup: false, link: routes.sources.toEditSource(next.id) })
					}
					return agg
				}, [])
				setSources([system, user])
				getStates()
			})
			.catch(() => {
				notification.error({ message: 'Не удалось получить список источников' })
			})
	}

	const getStates = () => {
		api.systemGetSourcesStates().then((res) => {
			setStates(res.data)
		})
	}

	const createSource = () => {
		api
			.sourcesCreate()
			.then((res) => {
				load()
				notification.success({ message: 'Создан источник ' + res.data.name })
			})
			.catch(() => notification.error({ message: 'Не удалось создать источник' }))
	}

	const columns: TableColumnsType<DataCell> = [
		{
			dataIndex: 'name',
			title: 'Название',
			width: '20em',
			render: (_, record) =>
				record.isGroup ? (
					<>{record.name}</>
				) : (
					<NavLink className='table-row' to={record.link} key={record.id}>
						<Button size='small'>{record.name}</Button>
					</NavLink>
				),
		},
		{
			dataIndex: 'isDisabled',
			title: 'Активность',
			width: '8em',
			render: (flag, record) =>
				record.isGroup ? <></> : !flag ? <Tag color='green'>Запущен</Tag> : <Tag>Остановлен</Tag>,
		},
		{
			title: 'Подключение',
			width: '10em',
			render: (_, record) => {
				if (record.isGroup) return <></>
				const state = states[record.id]
				if (!state) return <Tag>?</Tag>
				return (
					<span>
						{!state.isTryConnected ? (
							<Tag icon={<DisconnectOutlined />} color='default' title='Попыток подключения не было'>
								не исп.
							</Tag>
						) : state.isConnected ? (
							<Tag
								icon={<CheckOutlined />}
								color='success'
								title={'Последнее подключение: ' + timeAgo.format(new Date(state.lastTry))}
							>
								есть
							</Tag>
						) : (
							<Tag
								icon={<DisconnectOutlined />}
								color='error'
								title={
									(state.lastConnection
										? 'Последний раз связь была ' + timeAgo.format(new Date(state.lastConnection))
										: 'Успешных подключений не было с момента запуска') +
									'. Последняя попытка: ' +
									timeAgo.format(new Date(state.lastTry))
								}
							>
								нет
							</Tag>
						)}
					</span>
				)
			},
		},
		{
			dataIndex: 'id',
			title: <span title='Отображает общее количество тегов'>Теги</span>,
			width: '5em',
			render: (id, record) => {
				if (record.isGroup) return <></>
				const state = states[id]
				const tagsCount = state?.valuesAfterWriteSeconds.length ?? 0
				return tagsCount > 0 ? <span>{tagsCount}</span> : <span>нет</span>
			},
		},
		{
			dataIndex: 'type',
			title: 'Тип источника',
			width: '10em',
			render: (type, record) => (record.isGroup ? <></> : <>{getSourceTypeName(type)}</>),
		},
		{
			dataIndex: 'description',
			title: 'Описание',
		},
		{
			title: 'Новые значения за последние полчаса',
			width: '11em',
			render: (_, record) => {
				if (record.isGroup) return <></>
				const state = states[record.id]
				const tagsLastHalfHourCount = state?.valuesAfterWriteSeconds.filter((x) => x <= 1800).length ?? 0
				return <Tag color={tagsLastHalfHourCount > 0 ? 'success' : 'default'}>{tagsLastHalfHourCount}</Tag>
			},
		},
		{
			title: 'Новые значения за последние сутки',
			width: '11em',
			render: (_, record) => {
				if (record.isGroup) return <></>
				const state = states[record.id]
				const tagsLastDayCount = state?.valuesAfterWriteSeconds.filter((x) => x <= 86400).length ?? 0
				return <Tag color={tagsLastDayCount > 0 ? 'success' : 'default'}>{tagsLastDayCount}</Tag>
			},
		},
	]

	useEffect(load, [])
	useInterval(getStates, 5000)

	return (
		<>
			<PageHeader
				right={user.hasGlobalAccess(AccessType.Admin) && <Button onClick={createSource}>Добавить источник</Button>}
			>
				Зарегистрированные источники данных
			</PageHeader>
			<Table
				dataSource={sources}
				columns={columns}
				size='small'
				pagination={false}
				expandable={{
					expandedRowKeys: [-1000, -2000],
					defaultExpandAllRows: true,
					expandRowByClick: false,
					showExpandColumn: false,
				}}
				rowKey='id'
			/>
		</>
	)
})

export default SourcesList
