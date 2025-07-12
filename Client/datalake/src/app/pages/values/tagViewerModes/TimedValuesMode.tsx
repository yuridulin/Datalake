import { TagQuality } from '@/api/swagger/data-contracts'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { TransformedData } from '@/app/pages/values/types/TransformedData'
import compareValues from '@/functions/compareValues'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'

const TimedValuesMode = ({ relations }: TagViewerModeProps) => {
	const tagsValuesByTime = Object.values(
		relations.reduce((acc: Record<string, TransformedData>, { relationId, value }) => {
			// Добавляем проверку на существование value
			if (!value || !value.values) return acc

			const { id, guid, name, type, values: tagValues } = value

			tagValues.forEach(({ date, dateString, value, quality }) => {
				if (!acc[date]) acc[date] = { time: dateString, dateString }
				// Используем relationId как уникальный ключ
				acc[date][`${relationId}`] = {
					id,
					guid,
					name,
					type,
					value,
					quality,
				}
			})
			return acc
		}, {}),
	)

	const timeWithTagsColumns = [
		{
			title: 'Время',
			dataIndex: 'time',
			key: 'time',
			sorter: (a: TransformedData, b: TransformedData) => compareValues(a.time, b.time),
			showSorterTooltip: false,
		},
		...relations.map(({ relationId, value: x }) => ({
			title: x.localName, // Используем локальное имя из связи
			key: `${relationId}`,
			dataIndex: `${relationId}`,
			render: (value: { quality: TagQuality; value: TagValue } | undefined) => (
				<TagCompactValue type={x.type} value={value?.value ?? null} quality={value?.quality ?? TagQuality.Bad} />
			),
		})),
	]

	console.log(tagsValuesByTime, timeWithTagsColumns)

	return <Table columns={timeWithTagsColumns} dataSource={tagsValuesByTime} size='small' rowKey='date' />
}

export default TimedValuesMode
