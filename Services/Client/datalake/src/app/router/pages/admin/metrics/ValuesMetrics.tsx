import { KeyValuePairOfValuesRequestKeyAndValuesRequestUsageInfo as Metric } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useCallback, useEffect, useState } from 'react'

const columns: ColumnsType<Metric> = [
	{
		key: 1,
		dataIndex: ['key', 'requestKey'],
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
	const [metrics, setMetrics] = useState<Metric[]>([])

	const getMetrics = useCallback(() => {
		setLoading(true)
		store.api
			.statesGetValues()
			.then((res) => setMetrics(res.data))
			.catch(() => setMetrics([]))
			.finally(() => setLoading(false))
	}, [store.api])

	useEffect(getMetrics, [getMetrics])

	return <Table size='small' bordered={false} dataSource={metrics} columns={columns} loading={loading} />
}

export default ValuesMetrics
