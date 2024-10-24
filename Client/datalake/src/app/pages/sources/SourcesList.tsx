import { CheckOutlined, DisconnectOutlined } from '@ant-design/icons'
import { Button, Table, TableColumnsType, Tag } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { timeAgo } from '../../../api/extensions/timeAgoInstance'
import getSourceTypeName from '../../../api/models/getSourceTypeName'
import api from '../../../api/swagger-api'
import { SourceInfo, SourceState } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'

export default function SourcesList() {
	const [list, setList] = useState([] as SourceInfo[])
	const [states, setStates] = useState({} as { [key: number]: SourceState })

	const load = () => {
		api.sourcesReadAll({ withCustom: false }).then((res) => {
			setList(res.data)
		})
		getStates()
	}

	const getStates = () => {
		api.systemGetSources().then((res) => {
			setStates(res.data)
		})
	}

	const createSource = () => {
		api.sourcesCreate().then(load)
	}

	const columns: TableColumnsType<SourceInfo> = [
		{
			dataIndex: 'name',
			title: 'Название',
			width: '30em',
			render: (_, record) => (
				<NavLink
					className='table-row'
					to={routes.sources.toEditSource(record.id)}
					key={record.id}
				>
					<Button size='small'>{record.name}</Button>
				</NavLink>
			),
		},
		{
			title: 'Подключение',
			width: '10em',
			render: (_, record) => {
				const state = states[record.id]
				if (!state) return <></>
				return (
					<span>
						{state.isConnected ? (
							<Tag
								icon={<CheckOutlined />}
								color='success'
								title={
									'Последнее подключение: ' +
									timeAgo.format(new Date(state.lastTry))
								}
							>
								есть
							</Tag>
						) : (
							<Tag
								icon={<DisconnectOutlined />}
								color='error'
								title={
									(state.lastConnection
										? 'Последний раз связь была ' +
										  timeAgo.format(
												new Date(state.lastConnection),
										  )
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
			dataIndex: 'type',
			title: 'Тип источника',
			width: '10em',
			render: (type) => <>{getSourceTypeName(type)}</>,
		},
		{
			dataIndex: 'description',
			title: 'Описание',
		},
	]

	useEffect(load, [])
	useInterval(getStates, 5000)

	return (
		<>
			<PageHeader
				right={
					<Button onClick={createSource}>Добавить источник</Button>
				}
			>
				Зарегистрированные источники данных
			</PageHeader>
			<Table
				dataSource={list}
				columns={columns}
				size='small'
				pagination={false}
				rowKey='id'
			/>
		</>
	)
}
