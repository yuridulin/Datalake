import { SourceType, TagQuality, TagResolution, TagType, ValueRecord } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/TagCompactValue'
import { ExcelExportModeHandles, getQualityStyle } from '@/app/components/values/functions/exportExcel'
import { TagViewerModeProps } from '@/app/pages/values/types/TagViewerModeProps'
import { TagTypeName } from '@/functions/getTagTypeName'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'
import Column from 'antd/lib/table/Column'
import ExcelJS from 'exceljs'
import { saveAs } from 'file-saver'
import { forwardRef, useImperativeHandle } from 'react'

type ExactValuesRowType = {
	relationId: number
	id: number
	guid: string
	localName: string
	type: TagType
	value: TagValue
	quality: TagQuality
	resolution: TagResolution
	sourceType: SourceType
	date: string
}

const ExactValuesMode = forwardRef<ExcelExportModeHandles, TagViewerModeProps>(({ relations }, ref) => {
	const exactValues: ExactValuesRowType[] = relations.map(({ relationId, value: x }) => {
		const valueObject: ValueRecord = x.values.length
			? x.values[0]
			: { date: '', dateString: '', quality: TagQuality.BadNoValues, value: null }

		return {
			relationId,
			id: x.id,
			resolution: x.resolution,
			guid: x.guid,
			localName: x.localName,
			type: x.type,
			value: valueObject.value,
			quality: valueObject.quality,
			sourceType: x.sourceType,
			date: valueObject.dateString,
		} as ExactValuesRowType
	})

	useImperativeHandle(
		ref,
		() => ({
			exportToExcel: async () => {
				const workbook = new ExcelJS.Workbook()
				const worksheet = workbook.addWorksheet('Точные значения')

				// Заголовки
				worksheet.columns = [
					{ header: 'Тег', key: 'tag', width: 25 },
					{ header: 'Тип', key: 'type', width: 15 },
					{ header: 'Время записи', key: 'date', width: 20 },
					{ header: 'Значение', key: 'value', width: 20 },
				]

				// Данные
				exactValues.forEach((tag) => {
					const value = tag.value !== null ? String(tag.value) : ''
					const qualityValue = tag.quality ?? TagQuality.Bad

					const row = worksheet.addRow({
						tag: tag.localName,
						type: TagTypeName[tag.type],
						date: tag.date,
						value: `${value}`,
						quality: qualityValue,
					})

					const valueCell = row.getCell('value')
					valueCell.style = getQualityStyle(qualityValue)
				})

				// Сохранение файла
				const buffer = await workbook.xlsx.writeBuffer()
				saveAs(new Blob([buffer]), 'Точные_значения.xlsx')
			},
		}),
		[exactValues],
	)

	return (
		<>
			<Table dataSource={exactValues} size='small' rowKey='relationId'>
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
				<Column title='Время записи' dataIndex='value' render={(_, row: ExactValuesRowType) => row.date} />
			</Table>
		</>
	)
})

export default ExactValuesMode
