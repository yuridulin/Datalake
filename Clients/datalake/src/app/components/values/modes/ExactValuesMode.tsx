import TagButton from '@/app/components/buttons/TagButton'
import { ExcelExportModeHandles, getQualityStyle } from '@/app/components/values/functions/exportExcel'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagViewerModeProps } from '@/app/router/pages/values/types/TagViewerModeProps'
import { TagTypeName } from '@/functions/getTagTypeName'
import { SourceType, TagQuality, TagResolution, TagType, ValueRecord } from '@/generated/data-contracts'
import { Table } from 'antd'
import Column from 'antd/lib/table/Column'
import ExcelJS from 'exceljs'
import { saveAs } from 'file-saver'
import { forwardRef, useImperativeHandle } from 'react'
import { useLocalStorage } from 'react-use'

type ExactValuesRowType = {
	relationId: string
	id: number
	guid: string
	localName: string
	type: TagType
	number: number | null | undefined
	text: string | null | undefined
	boolean: boolean | null | undefined
	quality: TagQuality
	resolution: TagResolution
	sourceType: SourceType
	sourceId?: number
	accessRule?: { ruleId: number; access: number }
	date: string
}

const ExactValuesMode = forwardRef<ExcelExportModeHandles, TagViewerModeProps>(({ relations }, ref) => {
	const [paginationConfig, setPaginationConfig] = useLocalStorage('exactValuesPagination', {
		pageSize: 10,
		current: 1,
	})

	const exactValues: ExactValuesRowType[] = relations.map(({ relationId, value: x }) => {
		const valueObject: ValueRecord = x.values.length ? x.values[0] : { date: '', quality: TagQuality.BadNoValues }

		return {
			relationId,
			id: x.id,
			resolution: x.resolution,
			guid: x.guid,
			localName: x.localName,
			type: x.type,
			number: valueObject.number,
			text: valueObject.text,
			boolean: valueObject.boolean,
			quality: valueObject.quality,
			sourceType: x.sourceType,
			sourceId: 'sourceId' in x ? (x as { sourceId?: number }).sourceId : undefined,
			accessRule: 'accessRule' in x ? (x as { accessRule?: { ruleId: number; access: number } }).accessRule : undefined,
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
					const value = tag.text ?? tag.number ?? tag.boolean ?? ''
					const qualityValue = tag.quality ?? TagQuality.Unknown

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
			<Table
				dataSource={exactValues}
				size='small'
				rowKey='relationId'
				pagination={{
					pageSize: paginationConfig?.pageSize,
					current: paginationConfig?.current,
					showSizeChanger: true,
					onChange: (page, pageSize) => {
						setPaginationConfig({ current: page, pageSize: pageSize || 10 })
					},
				}}
			>
				<Column
					title='Тег'
					dataIndex='guid'
					width='25%'
					render={(_, row: ExactValuesRowType) => (
						<TagButton
							tag={{
								...row,
								name: row.localName,
								sourceId: row.sourceId ?? 0,
								accessRule: row.accessRule ?? { ruleId: 0, access: 0 },
							}}
						/>
					)}
				/>
				<Column
					title='Значение'
					dataIndex='value'
					render={(_, row: ExactValuesRowType) => (
						<TagCompactValue type={row.type} record={row} quality={row.quality ?? TagQuality.Unknown} />
					)}
				/>
				<Column title='Время записи' dataIndex='value' render={(_, row: ExactValuesRowType) => row.date} />
			</Table>
		</>
	)
})

export default ExactValuesMode
