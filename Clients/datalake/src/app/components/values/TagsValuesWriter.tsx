import TagButton from '@/app/components/buttons/TagButton'
import { TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagValueWithInfo } from '@/app/router/pages/values/types/TagValueWithInfo'
import { deserializeDate, serializeDate } from '@/functions/dateHandle'
import { URL_PARAMS } from '@/functions/urlParams'
import {
	SourceType,
	TagQuality,
	TagType,
	ValueRecord,
	ValueResult,
	ValueWriteRequest,
} from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { TagValue } from '@/types/tagValue'
import { CloseOutlined, PlaySquareOutlined } from '@ant-design/icons'
import { Button, Checkbox, DatePicker, Divider, Input, InputNumber, Space, Spin, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

type ExactValue = TagValueWithInfo & {
	value: ValueRecord
	relationId: string
	newValue?: TagValue
	hasNewValue: boolean
}

interface TagsValuesWriterProps {
	relations: string[] // Массив ID связей
	tagMapping: TagMappingType // Маппинг отношений
	integrated?: boolean // Скрываем часть контролов и инициируем запросы сразу после изменения настроек
}

const TagsValuesWriter = observer(({ relations, tagMapping, integrated = false }: TagsValuesWriterProps) => {
	const store = useAppStore()
	const [searchParams, setSearchParams] = useSearchParams()
	const [values, setValues] = useState<ExactValue[]>([])
	const [loading, setLoading] = useState(false)
	const [initialLoadDone, setInitialLoadDone] = useState(false)

	// Чтение даты из URL параметров
	const [exactDate, setExactDate] = useState<Dayjs | null | undefined>(
		deserializeDate(searchParams.get(URL_PARAMS.WRITER_DATE)),
	)

	// Обновление URL при изменении даты
	useEffect(() => {
		if (!integrated) {
			const newSearchParams = new URLSearchParams(searchParams)
			if (exactDate) {
				newSearchParams.set(URL_PARAMS.WRITER_DATE, serializeDate(exactDate)!)
			} else {
				newSearchParams.delete(URL_PARAMS.WRITER_DATE)
			}
			setSearchParams(newSearchParams, { replace: true })
		}
	}, [exactDate, integrated, searchParams, setSearchParams])

	const getValues = useCallback(() => {
		setLoading(true)
		if (relations.length === 0) {
			setLoading(false)
			return setValues([])
		}

		const tagIds = Array.from(new Set(relations.map((relId) => tagMapping[relId]?.id).filter(Boolean)))

		store.api
			.dataValuesGet([
				{
					requestKey: CLIENT_REQUESTKEY,
					tagsId: tagIds,
					exact: exactDate ? serializeDate(exactDate) : null,
				},
			])
			.then((res) => {
				const tagValuesMap = new Map<number, ValueRecord[]>()
				res.data[0].tags.forEach((tag) => {
					tagValuesMap.set(tag.id, tag.values)
				})

				const newValues = relations
					.filter((relId) => tagMapping[relId])
					.map((relId) => {
						const tagInfo = tagMapping[relId]
						const tagValues = tagValuesMap.get(tagInfo.id) || []
						const tagValue = tagValues?.[0]

						return {
							...tagInfo,
							relationId: relId,
							values: [],
							result: ValueResult.Ok,
							value: tagValue,
							newValue: tagValue?.value,
							hasNewValue: false,
						} as ExactValue
					})

				setValues(newValues)
			})
			.catch(console.error)
			.finally(() => setLoading(false))
	}, [exactDate, relations, tagMapping, store.api])

	// Автоматический запрос при инициализации, если есть настройки из URL
	useEffect(() => {
		if (!initialLoadDone && relations.length > 0 && exactDate) {
			getValues()
			setInitialLoadDone(true)
		}
	}, [relations, exactDate, initialLoadDone, getValues])

	const writeValues = () => {
		const valuesToWrite = values.reduce(
			(acc, next) => {
				if (acc[next.id]) return acc
				acc[next.id] = {
					date: exactDate ? serializeDate(exactDate) : null,
					id: next.id,
					value: next.newValue,
					quality: TagQuality.GoodManualWrite,
				}
				return acc
			},
			{} as Record<number, ValueWriteRequest>,
		)
		store.api.dataValuesWrite(Object.values(valuesToWrite)).then(getValues)
	}

	const setNewValue = (id: number, newValue: TagValue) => {
		setValues(values.map((x) => (x.id != id ? x : { ...x, newValue, hasNewValue: x.value.value !== newValue })))
	}

	// Автоматический запрос в интегрированном режиме
	useEffect(() => {
		if (integrated && relations.length > 0) {
			getValues()
		}
	}, [integrated, relations, exactDate, getValues])

	return (
		<>
			<Space>
				<DatePicker
					showTime
					placeholder='Выбрать момент'
					value={exactDate}
					onChange={setExactDate}
					disabled={relations.length === 0}
				/>
				{!integrated && (
					<Button
						onClick={getValues}
						disabled={loading || relations.length === 0}
						style={{ width: '9em', textAlign: 'left' }}
						icon={loading ? <Spin size='small' /> : <PlaySquareOutlined />}
					>
						Запрос
					</Button>
				)}
				<Button
					onClick={writeValues}
					icon={<PlaySquareOutlined />}
					disabled={values.filter((x) => x.hasNewValue).length === 0}
				>
					Запись
				</Button>
			</Space>

			<Divider orientation='left'>Значения</Divider>
			{!values.length ? (
				<>Выберите теги для редактирования</>
			) : (
				<>
					<Table size='small' rowKey='relationId' dataSource={values} pagination={false}>
						<Column<ExactValue>
							title='Тег'
							render={(_, record) => {
								return <TagButton tag={record} />
							}}
						/>
						<Column<ExactValue>
							title='Текущее значение'
							render={(_, record) => (
								<TagCompactValue type={record.type} value={record.value.value} quality={record.value.quality} />
							)}
						/>
						<Column<ExactValue>
							title='Новое значение'
							render={(_, record) => {
								if (record.sourceType !== SourceType.Manual) return <>не мануальный тег</>

								return (
									<>
										{record.type === TagType.Boolean ? (
											<Checkbox
												checked={
													record.newValue === null || record.newValue === undefined
														? undefined
														: Boolean(record.newValue)
												}
												onChange={(e) => setNewValue(record.id, e.target.checked ? 1 : 0)}
											/>
										) : record.type === TagType.Number ? (
											<InputNumber
												size='small'
												value={
													record.newValue === null || record.newValue === undefined
														? undefined
														: Number(record.newValue)
												}
												onChange={(e) => setNewValue(record.id, e)}
												placeholder='Введите новое значение'
											/>
										) : record.type === TagType.String ? (
											<Input
												value={
													record.newValue === null || record.newValue === undefined
														? undefined
														: String(record.newValue)
												}
												onChange={(e) => setNewValue(record.id, e.target.value)}
												placeholder='Введите новое значение'
											/>
										) : (
											<></>
										)}
										<Button size='small' onClick={() => setNewValue(record.id, null)} icon={<CloseOutlined />}></Button>
									</>
								)
							}}
						/>
						<Column<ExactValue>
							title='Ожидает записи'
							render={(_, record) =>
								record.sourceType === SourceType.Manual ? <Checkbox checked={record.hasNewValue} disabled /> : <></>
							}
						/>
					</Table>
				</>
			)}
		</>
	)
})

export default TagsValuesWriter
