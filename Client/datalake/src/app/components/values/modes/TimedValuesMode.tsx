import { TagQuality } from '@/api/swagger/data-contracts'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'
import { TransformedData } from '@/app/pages/values/types/TransformedData'
import compareValues from '@/functions/compareValues'
import { TagValue } from '@/types/tagValue'
import { Table } from 'antd'
import { ColumnType } from 'antd/es/table'

type TimedValuesModeProps = {
	relations: {
		relationId: number
		value: TagValueWithInfo
	}[]
	locf: boolean // признак, что нужно выполнять протягивание
}

const TimedValuesMode = ({ relations, locf }: TimedValuesModeProps) => {
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

	return <Table columns={columns} dataSource={rows} size='small' rowKey='time' showSorterTooltip={false} />
}

export default TimedValuesMode
