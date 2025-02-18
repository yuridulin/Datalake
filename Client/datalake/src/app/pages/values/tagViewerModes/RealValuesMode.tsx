import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { Table } from 'antd'

const RealValuesMode = ({ values }: TagViewerModeProps) => {

	// Преобразование данных
	const transformedData = mappedTags.reduce(
		(
			acc: Record<string, TransformedData>,
			{ id, guid, name, type, values },
		) => {
			values.forEach(
				({ date, dateString, value, quality }) => {
					if (!acc[date])
						acc[date] = { time: dateString, dateString }
					acc[date][guid] = {
						id,
						guid,
						name,
						type,
						value,
						quality,
					}
				},
			)
			return acc
		},
		{},
	)

	const dataSource = Object.values(transformedData)
	setRangeValues(dataSource)

	// Определение столбцов
	const columns = [
		{
			title: 'Время',
			dataIndex: 'time',
			key: 'time',
			sorter: (a, b) => compareValues(a.time, b.time),
			showSorterTooltip: false,
		},
		...mappedTags.map((x) => ({
			title: x.localName,
			key: x.guid,
			dataIndex: x.guid,
			render: (value: {
				quality: TagQuality
				value: TagValue
			}) => (
				<TagCompactValue
					type={x.type}
					value={value?.value ?? null}
					quality={value?.quality ?? TagQuality.Bad}
				/>
			),
		})),
	])

	return (
		<Table
			columns={rangeColumns}
			dataSource={rangeValues}
			size='small'
			rowKey='date'
		/>
	)
}
