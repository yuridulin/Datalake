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
				// Преобразуем массив TagUsageInfo в объект, группируя по requestKey
				const usageByRequestKey: Record<string, { requests: string[]; lastExecutionTime: string }> = {}

				res.data.forEach((tagUsage) => {
					if (tagUsage.requests) {
						Object.entries(tagUsage.requests).forEach(([requestKey, dateTime]) => {
							if (!usageByRequestKey[requestKey]) {
								usageByRequestKey[requestKey] = { requests: [], lastExecutionTime: '' }
							}
							usageByRequestKey[requestKey].requests.push(dateTime)
							// Берем последнее время выполнения
							if (!usageByRequestKey[requestKey].lastExecutionTime || dateTime > usageByRequestKey[requestKey].lastExecutionTime) {
								usageByRequestKey[requestKey].lastExecutionTime = dateTime
							}
						})
					}
				})

				// Преобразуем объект в массив для отображения в таблице
				const metricsArray: MetricItem[] = Object.entries(usageByRequestKey).map(([requestKey, data]) => ({
					key: { requestKey },
					value: {
						requestsLast24h: String(data.requests.length),
						lastExecutionTime: data.lastExecutionTime || '0',
						lastValuesCount: '0', // Эта информация недоступна в TagUsageInfo
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
