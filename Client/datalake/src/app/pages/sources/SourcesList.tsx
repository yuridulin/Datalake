import api from '@/api/swagger-api'
import { AccessType, SourceInfo, SourceState } from '@/api/swagger/data-contracts'
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

const SourcesList = observer(() => {
	const [sources, setSources] = useState([] as SourceInfo[])
	const [states, setStates] = useState({} as Record<string, SourceState>)

	const load = () => {
		setSources([])
		api
			.sourcesReadAll({ withCustom: false })
			.then((res) => {
				setSources(res.data)
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
				setSources([...sources, res.data])
				notification.success({ message: 'Создан источник ' + res.data.name })
			})
			.catch(() => notification.error({ message: 'Не удалось создать источник' }))
	}

	const columns: TableColumnsType<SourceInfo> = [
		{
			dataIndex: 'name',
			title: 'Название',
			width: '30em',
			render: (_, record) => (
				<NavLink className='table-row' to={routes.sources.toEditSource(record.id)} key={record.id}>
					<Button size='small'>{record.name}</Button>
				</NavLink>
			),
		},
		{
			title: 'Подключение',
			width: '10em',
			render: (_, record) => {
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
			render: (id) => {
				const state = states[id]
				const tagsCount = state?.valuesAfterWriteSeconds.length ?? 0
				return tagsCount > 0 ? <span>{tagsCount}</span> : <span>нет</span>
			},
		},
		{
			dataIndex: 'type',
			title: 'Тип источника',
			width: '10em',
			render: (type) => <>{getSourceTypeName(type)}</>,
		},
		{
			dataIndex: 'description',
			title: 'Описание',
		},
		{
			title: 'Новые значения за последние полчаса',
			width: '11em',
			render: (_, record) => {
				const state = states[record.id]
				const tagsLastHalfHourCount = state?.valuesAfterWriteSeconds.filter((x) => x <= 1800).length ?? 0
				return <Tag color={tagsLastHalfHourCount > 0 ? 'success' : 'default'}>{tagsLastHalfHourCount}</Tag>
			},
		},
		{
			title: 'Новые значения за последние сутки',
			width: '11em',
			render: (_, record) => {
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
			<Table dataSource={sources} columns={columns} size='small' pagination={false} rowKey='id' />
		</>
	)
})

export default SourcesList
