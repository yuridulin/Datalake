import { TagQuality } from '@/api/swagger/data-contracts'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { TransformedData } from '@/app/pages/values/types/TransformedData'
import compareValues from '@/functions/compareValues'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'

const TimedValuesMode = ({ values }: TagViewerModeProps) => {
	const tagsValuesByTime = Object.values(
		values.reduce((acc: Record<string, TransformedData>, { id, guid, name, type, values }) => {
			values.forEach(({ date, dateString, value, quality }) => {
				if (!acc[date]) acc[date] = { time: dateString, dateString }
				acc[date][guid] = {
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
		...values.map((x) => ({
			title: x.localName,
			key: x.guid,
			dataIndex: x.guid,
			render: (value: { quality: TagQuality; value: TagValue }) => (
				<TagCompactValue type={x.type} value={value?.value ?? null} quality={value?.quality ?? TagQuality.Bad} />
			),
		})),
	]

	console.log(tagsValuesByTime, timeWithTagsColumns)

	return <Table columns={timeWithTagsColumns} dataSource={tagsValuesByTime} size='small' rowKey='date' />
}

export default TimedValuesMode
