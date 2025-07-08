import { SourceType, TagFrequency, TagQuality, TagType, ValueRecord } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'
import Column from 'antd/lib/table/Column'

type ExactValuesRowType = {
	id: number
	guid: string
	localName: string
	type: TagType
	value: TagValue
	quality: TagQuality
	frequency: TagFrequency
	sourceType: SourceType
}

const ExactValuesMode = ({ values }: TagViewerModeProps) => {
	const exactValues: ExactValuesRowType[] = values.map((x) => {
		const valueObject: ValueRecord = x.values.length
			? x.values[0]
			: { date: '', dateString: '', quality: TagQuality.BadNoValues, value: null }
		return {
			id: x.id,
			frequency: x.frequency,
			guid: x.guid,
			localName: x.localName,
			type: x.type,
			value: valueObject.value,
			quality: valueObject.quality,
			sourceType: x.sourceType,
		}
	})

	return (
		<Table dataSource={exactValues} size='small' rowKey='guid'>
			<Column
				title='Тег'
				dataIndex='guid'
				width='25%'
				render={(_, row: ExactValuesRowType) => <TagButton tag={{ ...row, name: row.localName }} />}
			/>
			<Column
				title='Значение'
				dataIndex='value'
				render={(_, row: ExactValuesRowType) => (
					<TagCompactValue type={row.type} value={row.value ?? null} quality={row.quality ?? TagQuality.Bad} />
				)}
			/>
		</Table>
	)
}

export default ExactValuesMode
