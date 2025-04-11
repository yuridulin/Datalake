import api from '@/api/swagger-api'
import { HistoryReadMetric } from '@/api/swagger/data-contracts'
import { Button, notification, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'

const columns: ColumnsType<HistoryReadMetric> = [
	{
		title: 'Дата выполнения',
		dataIndex: 'date',
		render: (date: string) => dayjs(date).format('HH:mm:ss'),
	},
	{
		title: 'Теги',
		dataIndex: 'tagsId',
		render: (tags: number[], row: HistoryReadMetric) => (
			<Button
				size='small'
				title='Нажмите, чтобы скопировать SQL текст'
				onClick={() => navigator.clipboard.writeText(row.sql || '')}
			>
				{tags.length}
			</Button>
		),
	},
	{
		title: 'Диапазон',
		dataIndex: 'old',
		render: (old: string, row: HistoryReadMetric) => (
			<div>
				{old} :: {row.young}
			</div>
		),
	},
	{
		title: 'Время выполнения',
		dataIndex: 'elapsed',
	},
	{
		title: 'Количество записей',
		dataIndex: 'recordsCount',
	},
]

const HistoryReadMetrics = () => {
	const [metrics, setMetrics] = useState([] as HistoryReadMetric[])
	const [pagination, setPagination] = useState({ current: 1, pageSize: 100 })

	const initialLoad = () => {
		api
			.systemGetReadMetrics()
			.then((res) => setMetrics(res.data.reverse()))
			.catch(() => {
				notification.error({ message: 'Не удалось прочитать список метрик' })
			})
	}

	useEffect(initialLoad, [])

	return (
		<Table
			size='small'
			dataSource={metrics}
			columns={columns}
			rowKey='date'
			pagination={{
				...pagination,
				position: ['bottomCenter'],
				showSizeChanger: false,
				onChange: (page, pageSize) => setPagination({ current: page, pageSize }),
			}}
			scroll={{ y: 'calc(100vh - 200px)' }}
		/>
	)
}

export default HistoryReadMetrics
