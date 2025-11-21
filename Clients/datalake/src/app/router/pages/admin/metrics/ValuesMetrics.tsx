import { useAppStore } from '@/store/useAppStore'
import { Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useRef, useState } from 'react'

type MetricItem = {
	key: {
		requestKey: string
	}
	value: {
		requestsLast24h: string
		lastExecutionTime: string
		lastValuesCount: string
	}
}

const columns: ColumnsType<MetricItem> = [
	{
		key: 1,
		dataIndex: 'key.requestKey',
		title: 'Запрос',
		render(_, record) {
			return <>{record.key?.requestKey}</>
		},
	},
	{
		key: 2,
		title: 'Кол-во запросов за сутки',
		render(_, record) {
			return <>{record.value?.requestsLast24h}</>
		},
	},
	{
		key: 3,
		title: 'Длительность последнего выполнения',
		render(_, record) {
			return <>{record.value?.lastExecutionTime}</>
		},
	},
	{
		key: 4,
		title: 'Кол-во значений в последнем запросе',
		render(_, record) {
			return <>{record.value?.lastValuesCount}</>
		},
	},
	{
		key: 5,
		title: 'Настройки',
		render(_, record) {
			return <table>{record.key?.requestKey}</table>
		},
	},
]

const ValuesMetrics = () => {
	const store = useAppStore()
	const [loading, setLoading] = useState<boolean>(false)
	const [metrics, setMetrics] = useState<MetricItem[]>([])
	const hasLoadedRef = useRef(false)

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true

		setLoading(true)
		store.api
			.dataTagsGetUsage({})
			.then((res) => {
				// Преобразуем объект в массив для отображения в таблице
				const metricsArray: MetricItem[] = Object.entries(res.data).map(([key, value]) => ({
					key: { requestKey: key },
					value: {
						requestsLast24h: value.requestsLast24h || '0',
						lastExecutionTime: value.lastExecutionTime || '0',
						lastValuesCount: value.lastValuesCount || '0',
					},
				}))
				setMetrics(metricsArray)
			})
			.catch(() => setMetrics([]))
			.finally(() => setLoading(false))
	}, [store.api])

	return <Table size='small' bordered={false} dataSource={metrics} columns={columns} loading={loading} />
}

export default ValuesMetrics
