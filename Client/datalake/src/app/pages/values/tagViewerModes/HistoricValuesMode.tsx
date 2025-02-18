import TagQualityEl from '@/app/components/TagQualityEl'
import TagValueEl from '@/app/components/TagValueEl'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { ValueType } from '@/app/pages/values/types/ValueType'
import { Table } from 'antd'
import Column from 'antd/lib/table/Column'

const HistoricValuesMode = ({ values }: TagViewerModeProps) => {
	return (
		<Table dataSource={values} size='small' rowKey='guid'>
			<Column
				title='Тег'
				dataIndex='guid'
				render={(_, row: ValueType) => row.localName}
			/>
			<Column
				width='40%'
				title='Значение'
				dataIndex='value'
				render={(value, row: ValueType) => (
					<TagValueEl
						type={row.type}
						guid={row.guid}
						allowEdit={true}
						value={value}
					/>
				)}
			/>
			<Column
				width='10em'
				title='Качество'
				dataIndex='quality'
				render={(quality) => <TagQualityEl quality={quality} />}
			/>
		</Table>
	)
}

export default HistoricValuesMode
