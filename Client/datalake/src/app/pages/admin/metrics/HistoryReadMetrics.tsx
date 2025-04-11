import api from '@/api/swagger-api'
import { HistoryReadMetricInfo } from '@/api/swagger/data-contracts'
import { Button, notification, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'

const columns: ColumnsType<HistoryReadMetricInfo> = [
	{
		title: 'Дата выполнения',
		dataIndex: 'date',
		sorter: (a, b) => dayjs(a.date).diff(dayjs(b.date)),
	},
	{
		title: 'Кол-во тегов',
		dataIndex: 'tagsId',
		render: (tags, row) => (
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
		dataIndex: 'timeSettings',
	},
	{
		title: 'Время выполнения',
		dataIndex: 'elapsed',
		sorter: (a, b) => a.milliseconds - b.milliseconds,
	},
	{
		title: 'Количество записей',
		dataIndex: 'recordsCount',
	},
]

const HistoryReadMetrics = () => {
	const [metrics, setMetrics] = useState([] as HistoryReadMetricInfo[])
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
			showSorterTooltip={false}
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
