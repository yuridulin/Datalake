import { TagSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { notification, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'

type TagWithMetric = {
	id: number
	name: string
	requests: {
		key: string
		date: string
	}[]
}

const columns: ColumnsType<TagWithMetric> = [
	{
		dataIndex: 'name',
		title: 'Тег',
	},
	{
		dataIndex: 'id',
		title: 'Был ли доступ',
		render: (_, row) => (row.requests.length > 0 ? 'Да' : 'Нет'),
	},
]

const TagsAccessMetrics = () => {
	const store = useAppStore()
	const [tagsMetrics, setTagsMetrics] = useState([] as TagWithMetric[])

	const initialLoad = () => {
		let tags: TagSimpleInfo[] = []
		let metrics: Record<string, Record<string, string>> = {}

		Promise.all([
			store.api
				.tagsGetAll()
				.then((res) => {
					tags = res.data
				})
				.catch(() => {
					notification.error({ message: 'Не удалось прочитать список тегов' })
				}),
			store.api
				.systemGetTagsStates()
				.then((res) => {
					metrics = res.data
				})
				.catch(() => {
					notification.error({ message: 'Не удалось прочитать метрики доступа к тегам' })
				}),
		])
			.then(() => {
				setTagsMetrics(
					tags.map((tag) => {
						const metric = metrics[tag.id]
						return {
							id: tag.id,
							name: tag.name,
							requests: metric ? Object.entries(metric).map((x) => ({ key: x[0], date: x[1] })) : [],
						}
					}),
				)
			})
			.catch((e) => {
				console.error(e)
				notification.error({ message: 'Ошибка при построении списка метрик' })
			})
	}

	useEffect(initialLoad, [])

	return (
		<Table
			size='small'
			dataSource={tagsMetrics}
			columns={columns}
			expandable={{
				expandedRowRender: (record) => (
					<p style={{ margin: 0 }}>
						{record.requests.map((x) => (
							<div>
								{x.key}: {x.date}
							</div>
						))}
					</p>
				),
				rowExpandable: (record) => record.requests.length > 0,
			}}
		/>
	)
}

export default TagsAccessMetrics
