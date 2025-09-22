import { ExcelExportModeHandles, getQualityStyle } from '@/app/components/values/functions/exportExcel'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagValueWithInfo } from '@/app/router/pages/values/types/TagValueWithInfo'
import { TransformedData } from '@/app/router/pages/values/types/TransformedData'
import compareValues from '@/functions/compareValues'
import { TagTypeName } from '@/functions/getTagTypeName'
import { TagQuality } from '@/generated/data-contracts'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'
import { ColumnType } from 'antd/es/table'
import ExcelJS from 'exceljs'
import saveAs from 'file-saver'
import { forwardRef, useImperativeHandle } from 'react'
import { useLocalStorage } from 'react-use'

type TimedValuesModeProps = {
	relations: {
		relationId: string
		value: TagValueWithInfo
	}[]
	locf: boolean // признак, что нужно выполнять протягивание
}

const TimedValuesMode = forwardRef<ExcelExportModeHandles, TimedValuesModeProps>(({ relations, locf }, ref) => {
	const [paginationConfig, setPaginationConfig] = useLocalStorage('timedValuesPagination', {
		pageSize: 10,
		current: 1,
	})

	// 1. Собираем и объединяем все точки по уникальному ключу date
	const dateMap = new Map<string, TransformedData>()

	relations.forEach(({ relationId, value }) => {
		if (!value?.values) return
		const { id, guid, name, type, values: tagValues } = value

		tagValues.forEach(({ date, dateString, value: tagVal, quality }) => {
			if (!dateMap.has(date)) {
				dateMap.set(date, { time: dateString, dateString })
			}
			dateMap.get(date)![String(relationId)] = {
				id,
				guid,
				name,
				type,
				value: tagVal,
				quality,
			}
		})
	})

	// 2. Переводим Map в отсортированный массив
	const rows = Array.from(dateMap.values()).sort((a, b) => compareValues(a.time, b.time))

	// 3. LOCF: пробегаем по строкам и заполняем пропуски из предыдущей строки
	if (locf && rows.length > 1) {
		for (let i = 1; i < rows.length; i++) {
			const prev = rows[i - 1]
			const curr = rows[i]

			relations.forEach(({ relationId }) => {
				const key = String(relationId)
				// если в текущей строке нет данных по этому тегу, но в предыдущей они есть
				if (curr[key] == null && prev[key] != null) {
					const orig = prev[key] as {
						id: number
						guid: string
						name: string
						type: string
						value: TagValue
						quality: number
					}
					curr[key] = {
						...orig,
						// задаём новое качество 200 или 100
						quality: orig.quality >= 192 ? 200 : 100,
					}
				}
			})
		}
	}
	// 4. Формируем колонки таблицы
	const columns: ColumnType<TransformedData>[] = [
		{
			title: 'Время',
			dataIndex: 'time',
			key: 'time',
			sorter: (a, b) => compareValues(a.time, b.time),
			defaultSortOrder: 'ascend',
		},
		...relations.map(({ relationId, value: meta }) => ({
			title: meta.localName,
			key: String(relationId),
			dataIndex: String(relationId),
			render: (cell: { value: TagValue; quality: TagQuality } | undefined) => (
				<TagCompactValue type={meta.type} value={cell?.value ?? null} quality={cell?.quality ?? TagQuality.Bad} />
			),
		})),
	]

	useImperativeHandle(
		ref,
		() => ({
			exportToExcel: async () => {
				const workbook = new ExcelJS.Workbook()
				const worksheet = workbook.addWorksheet('Исторические данные')

				// Заголовки
				const headerRow = ['Время']
				relations.forEach((rel) => {
					headerRow.push(`${rel.value.localName} [${TagTypeName[rel.value.type]}]`)
				})
				worksheet.addRow(headerRow)

				// Данные
				rows.forEach((row) => {
					const dataRow = [row.time]
					const qualities: (number | null)[] = []

					relations.forEach((rel) => {
						const key = String(rel.relationId)
						const cellData = row[key]

						if (cellData) {
							dataRow.push(`${cellData.value ?? ''}`)
							qualities.push(cellData.quality)
						} else {
							dataRow.push('')
							qualities.push(null)
						}
					})

					worksheet.addRow(dataRow)
				})

				// Применение стилей
				worksheet.eachRow((row, rowIndex) => {
					if (rowIndex > 1) {
						// Пропускаем заголовок
						for (let colIndex = 2; colIndex <= relations.length + 1; colIndex++) {
							const cell = row.getCell(colIndex)
							const value = cell.value as string

							if (value && value.includes(';')) {
								const parts = value.split(';')
								const quality = Number(parts[parts.length - 1])

								if (!isNaN(quality)) {
									cell.style = getQualityStyle(quality)
									cell.value = value // Сохраняем исходное значение
								}
							}
						}
					}
				})

				// Сохранение файла
				const buffer = await workbook.xlsx.writeBuffer()
				saveAs(new Blob([buffer]), 'Исторические_данные.xlsx')
			},
		}),
		[relations, rows],
	)

	return (
		<>
			<Table
				columns={columns}
				dataSource={rows}
				size='small'
				rowKey='time'
				showSorterTooltip={false}
				pagination={{
					pageSize: paginationConfig?.pageSize,
					current: paginationConfig?.current,
					showSizeChanger: true,
					onChange: (page, pageSize) => {
						setPaginationConfig({ current: page, pageSize: pageSize || 10 })
					},
				}}
			/>
		</>
	)
})

export default TimedValuesMode
