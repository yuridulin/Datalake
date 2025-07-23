import api from '@/api/swagger-api'
import { TagInputInfo, ValueRecord } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import TagCompactValue from '@/app/components/TagCompactValue'
import TagValueText from '@/app/components/TagValue'
import { useInterval } from '@/hooks/useInterval'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { Table } from 'antd'
import { useCallback, useEffect, useState } from 'react'

interface TagFormulaViewProps {
	formula: string | null | undefined
	inputs: TagInputInfo[]
}

type TagFormulaValues = Record<number, ValueRecord>

const TagFormulaView = ({ formula, inputs }: TagFormulaViewProps) => {
	const [values, setValues] = useState<TagFormulaValues>({})

	const renderFormulaWithValues = useCallback(
		(values: TagFormulaValues) => {
			if (!formula) return formula
			if (!inputs.length) return formula

			// Функция экранирования для RegExp
			const escapeRegExp = (str: string) => str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')

			// Подготовка вариантов для поиска: [name] и name
			const allPatterns: string[] = []
			inputs.forEach((input) => {
				allPatterns.push(`\\[${escapeRegExp(input.variableName)}\\]`)
				allPatterns.push(escapeRegExp(input.variableName))
			})

			// Сортировка по убыванию длины для приоритета длинных совпадений
			allPatterns.sort((a, b) => b.length - a.length)
			const regex = new RegExp(`(${allPatterns.join('|')})`)
			const parts = formula.split(regex)

			return parts.map((part, index) => {
				// Проверяем все возможные форматы параметра
				let variableName: string | null = null

				// Вариант 1: в квадратных скобках [name]
				if (part.startsWith('[') && part.endsWith(']')) {
					variableName = part.slice(1, -1).trim()
				}
				// Вариант 2: без скобок (name)
				else {
					variableName = part.trim()
				}

				// Ищем соответствующий входной параметр
				const input = inputs?.find((x) => x.variableName === variableName)

				if (input) {
					const valueInfo = values[input.id]
					let valueContent = <>?</>

					if (valueInfo) {
						valueContent = <TagValueText type={input.type} value={valueInfo.value} />
					}

					return valueContent
				}

				// Возвращаем обычный текст для не-параметров
				return <span key={index}>{part}</span>
			})
		},
		[formula, inputs],
	)

	const info: InfoTableProps['items'] = {
		Формула: renderFormulaWithValues(values),
	}

	const getValues = useCallback(() => {
		if (!inputs.length) return
		api.valuesGet([{ requestKey: CLIENT_REQUESTKEY, tagsId: inputs.map((x) => x.id) }]).then((res) => {
			const newValues = res.data[0].tags.reduce((acc, next) => {
				acc[next.id] = next.values[0]
				return acc
			}, {} as TagFormulaValues)
			setValues(newValues)
		})
	}, [inputs])

	useEffect(getValues, [getValues])
	useInterval(getValues, 5000)

	return formula ? (
		<>
			<InfoTable items={info} />
			<br />
			{inputs.length ? (
				<Table
					size='small'
					bordered
					indentSize={1}
					pagination={false}
					dataSource={inputs}
					rowKey={'relationId'}
					columns={[
						{
							key: 'name',
							dataIndex: 'variableName',
							title: 'Обозначение',
							width: '10em',
						},
						{
							key: 'value',
							title: 'Значение',
							width: '10em',
							render: (_, x) => {
								const value = values[x.id]
								return value ? <TagCompactValue type={x.type} quality={value.quality} value={value.value} /> : <>?</>
							},
						},
						{
							key: 'link',
							title: 'Используемый тег',
							render: (_, x) => <TagButton tag={x} />,
						},
					]}
				/>
			) : (
				<>Входные параметры не заданы</>
			)}
		</>
	) : (
		<>Формула пуста</>
	)
}

export default TagFormulaView
