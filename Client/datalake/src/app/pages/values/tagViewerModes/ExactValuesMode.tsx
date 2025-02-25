import { TagQuality, TagType } from '@/api/swagger/data-contracts'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'
import Column from 'antd/lib/table/Column'

type ExactValuesRowType = {
	guid: string
	localName: string
	type: TagType
	value: TagValue
	quality: TagQuality
}

const ExactValuesMode = ({ values }: TagViewerModeProps) => {
	const exactValues: ExactValuesRowType[] = values.map((x) => {
		const valueObject = x.values[0]
		return {
			guid: x.guid,
			localName: x.localName,
			type: x.type,
			value: valueObject.value,
			quality: valueObject.quality,
		}
	})

	return (
		<Table dataSource={exactValues} size='small' rowKey='guid'>
			<Column
				title='Тег'
				dataIndex='guid'
				width='25%'
				render={(_, row: ExactValuesRowType) => row.localName}
			/>
			<Column
				title='Значение'
				dataIndex='value'
				render={(_, row: ExactValuesRowType) => (
					<TagCompactValue
						type={row.type}
						value={row.value ?? null}
						quality={row.quality ?? TagQuality.Bad}
					/>
				)}
			/>
		</Table>
	)
}

export default ExactValuesMode
