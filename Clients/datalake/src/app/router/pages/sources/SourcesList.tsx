import PollingLoader from '@/app/components/loaders/PollingLoader'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import { timeAgo } from '@/functions/dateHandle'
import getSourceTypeName from '@/functions/getSourceTypeName'
import { AccessType, SourceActivityInfo, SourceInfo, SourceType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { CheckOutlined, DisconnectOutlined } from '@ant-design/icons'
import { Button, notification, Table, TableColumnsType, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { NavLink } from 'react-router-dom'

interface DataCell extends SourceInfo {
	isGroup: boolean
	children?: DataCell[]
	link: string
}

const ExcludedTypes = [SourceType.Unset, SourceType.System]
const NotEnteredTypes = [SourceType.Aggregated, SourceType.Calculated, SourceType.Manual]

const SourcesList = observer(() => {
	useDatalakeTitle('Источники')
	const store = useAppStore()
	const [sources, setSources] = useState([] as DataCell[])
	const [states, setStates] = useState({} as Record<number, SourceActivityInfo>)
	const hasLoadedRef = useRef(false)

	const getStates = useCallback(async () => {
		if (sources.length === 0) return
		const res = await store.api.dataSourcesGetActivity(sources.map((x) => x.id))
		setStates(
			res.data.reduce(
				(agg, next) => {
					agg[next.sourceId] = next
					return agg
				},
				{} as Record<number, SourceActivityInfo>,
			),
		)
	}, [store.api, sources])

	const load = useCallback(() => {
		setSources([])
		store.api
			.inventorySourcesGetAll({ withCustom: true })
			.then((res) => {
				const [system, user]: DataCell[] = [
					{
						isGroup: true,
						id: -2000,
						name: 'Системные источники',
						isDisabled: false,
						type: SourceType.Unset,
						children: [],
						link: '',
					},
					{
						isGroup: true,
						id: -1000,
						name: 'Пользовательские источники',
						isDisabled: false,
						type: SourceType.Unset,
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
				system.children = system.children?.sort((a, b) => a.name.localeCompare(b.name))
				user.children = user.children?.sort((a, b) => a.name.localeCompare(b.name))
				setSources([user, system])
			})
			.catch(() => {
				notification.error({ message: 'Не удалось получить список источников' })
			})
	}, [store.api])

	const createSource = useCallback(() => {
		store.api
			.inventorySourcesCreate()
			.then(() => {
				hasLoadedRef.current = false
				load()
				notification.success({ message: 'Источник создан' })
			})
			.catch(() => notification.error({ message: 'Не удалось создать источник' }))
	}, [store.api, load])

	const columns = useMemo(
		() =>
			[
				{
					dataIndex: 'name',
					title: 'Название',
					width: '20em',
					sorter: (a, b) => a.name.localeCompare(b.name),
					render: (_, record) =>
						record.isGroup ? (
							<i style={{ paddingLeft: '2em' }}>{record.name}</i>
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
					sorter: (a, b) => Number(a.isDisabled) - Number(b.isDisabled),
					render: (flag, record) =>
						record.isGroup ? <></> : !flag ? <Tag>Активен</Tag> : <Tag color='warning'>Остановлен</Tag>,
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
								{!state.lastTry ? (
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
					sorter: (a, b) => (states[a.id]?.valuesAll ?? 0) - (states[b.id]?.valuesAll ?? 0),
					render: (id, record) => {
						if (record.isGroup) return <></>
						const state = states[id]
						const tagsCount = state?.valuesAll ?? 0
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
					width: '13em',
					sorter: (a, b) => (states[a.id]?.valuesLastHalfHour ?? 0) - (states[b.id]?.valuesLastHalfHour ?? 0),
					render: (_, record) => {
						if (record.isGroup) return <></>
						const state = states[record.id]
						const tagsLastHalfHourCount = state?.valuesLastHalfHour ?? 0
						return <Tag color={tagsLastHalfHourCount > 0 ? 'success' : 'default'}>{tagsLastHalfHourCount}</Tag>
					},
				},
				{
					title: 'Новые значения за последние сутки',
					width: '13em',
					sorter: (a, b) => (states[a.id]?.valuesLastDay ?? 0) - (states[b.id]?.valuesLastDay ?? 0),
					render: (_, record) => {
						if (record.isGroup) return <></>
						const state = states[record.id]
						const tagsLastDayCount = state?.valuesLastDay ?? 0
						return <Tag color={tagsLastDayCount > 0 ? 'success' : 'default'}>{tagsLastDayCount}</Tag>
					},
				},
			] as TableColumnsType<DataCell>,
		[states],
	)

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true
		load()
	}, [load])

	// Загружаем состояния после того, как sources установлены
	useEffect(() => {
		if (sources.length > 0) {
			getStates()
		}
	}, [sources, getStates])

	return (
		<>
			<PageHeader
				right={[store.hasGlobalAccess(AccessType.Admin) && <Button onClick={createSource}>Добавить источник</Button>]}
			>
				Зарегистрированные источники данных
			</PageHeader>
			<PollingLoader pollingFunction={getStates} interval={5000} />
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
				showSorterTooltip={false}
				rowKey='id'
			/>
		</>
	)
})

export default SourcesList
