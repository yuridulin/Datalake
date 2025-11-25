import TagButton from '@/app/components/buttons/TagButton'
import { TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagValueWithInfo } from '@/app/router/pages/values/types/TagValueWithInfo'
import { deserializeDate, serializeDate } from '@/functions/dateHandle'
import { URL_PARAMS } from '@/functions/urlParams'
import {
	AccessRuleInfo,
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
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

type ExactValue = TagValueWithInfo & {
	value: ValueRecord
	relationId: string
	newValue?: TagValue
	hasNewValue: boolean
	sourceId?: number
	accessRule?: AccessRuleInfo
}

interface TagsValuesWriterProps {
	relations: string[] // Массив ID связей
	tagMapping: TagMappingType // Маппинг отношений
	integrated?: boolean // Скрываем часть контролов и инициируем запросы сразу после изменения настроек
}

const TagsValuesWriter = observer(({ relations, tagMapping, integrated = false }: TagsValuesWriterProps) => {
	const store = useAppStore()
	const [searchParams, setSearchParams] = useSearchParams()
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

	const tagIds = useMemo(
		() =>
			Array.from(
				new Set(
					relations
						.map((relId) => {
							const tagInfo = tagMapping[relId]
							return tagInfo?.tag?.id ?? tagInfo?.tagId
						})
						.filter((id): id is number => id !== null && id !== undefined),
				),
			),
		[relations, tagMapping],
	)

	const valuesRequest = useMemo(
		() =>
			tagIds.length > 0
				? [
						{
							requestKey: CLIENT_REQUESTKEY,
							tagsId: tagIds,
							exact: exactDate ? serializeDate(exactDate) : null,
						},
					]
				: null,
		[tagIds, exactDate],
	)

	// Получаем значения из store (реактивно через MobX)
	const valuesResponse = useMemo(
		() => (valuesRequest ? store.valuesStore.getValues(valuesRequest) : []),
		[valuesRequest, store.valuesStore],
	)

	// Преобразуем данные из store в формат компонента
	const valuesFromStore = useMemo(() => {
		if (valuesResponse.length === 0 || relations.length === 0) {
			return []
		}

		const tagValuesMap = new Map<number, ValueRecord[]>()
		valuesResponse[0]?.tags.forEach((tag) => {
			tagValuesMap.set(tag.id, tag.values)
		})

		return relations
			.filter((relId) => tagMapping[relId])
			.map((relId) => {
				const tagInfo = tagMapping[relId]
				const tagId = tagInfo.tag?.id ?? tagInfo.tagId ?? 0
				const tagValues = tagValuesMap.get(tagId) || []
				const tagValue = tagValues?.[0]
				const tag = tagInfo.tag

				if (!tag) {
					throw new Error(`Tag not found for relation ${relId}`)
				}

				return {
					...tagInfo,
					id: tag.id,
					guid: tag.guid,
					name: tag.name,
					type: tag.type,
					resolution: tag.resolution,
					sourceType: tag.sourceType,
					sourceId: tag.sourceId,
					accessRule: tag.accessRule,
					relationId: relId,
					values: [],
					result: ValueResult.Ok,
					value: tagValue,
					newValue: tagValue?.boolean ?? tagValue?.number ?? tagValue?.text,
					hasNewValue: false,
				} as ExactValue
			})
	}, [valuesResponse, relations, tagMapping])

	// Используем значения из store как основу, но храним локальные изменения
	const [localValues, setLocalValues] = useState<ExactValue[]>([])

	// Синхронизируем значения из store с локальным состоянием
	useEffect(() => {
		setLocalValues(valuesFromStore)
	}, [valuesFromStore])

	const values = localValues

	const getValues = useCallback(async () => {
		if (relations.length === 0) {
			return
		}

		if (!valuesRequest) return

		setLoading(true)
		try {
			await store.valuesStore.refreshValues(valuesRequest)
		} catch (error) {
			console.error('Failed to load values:', error)
		} finally {
			setLoading(false)
		}
	}, [relations, valuesRequest, store.valuesStore])

	// Автоматический запрос при инициализации, если есть настройки из URL
	useEffect(() => {
		if (!initialLoadDone && relations.length > 0 && exactDate) {
			getValues()
			setInitialLoadDone(true)
		}
	}, [relations, exactDate, initialLoadDone, getValues])

	const writeValues = async () => {
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
		try {
			await store.api.dataValuesWrite(Object.values(valuesToWrite))
			// Инвалидируем кэш значений для записанных тегов
			store.valuesStore.invalidateValues(tagIds)
			await getValues()
		} catch (error) {
			console.error('Failed to write values:', error)
		}
	}

	const setNewValue = (id: number, newValue: TagValue) => {
		setLocalValues(localValues.map((x) => (x.id != id ? x : { ...x, newValue, hasNewValue: x.value.text !== newValue }))) // TODO: неправильно же!
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
								return (
									<TagButton
										tag={{
											id: record.id,
											guid: record.guid,
											name: record.name,
											type: record.type,
											resolution: record.resolution,
											sourceType: record.sourceType,
											sourceId: record.sourceId ?? 0,
											accessRule: record.accessRule ?? { ruleId: 0, access: 0 },
										}}
									/>
								)
							}}
						/>
						<Column<ExactValue>
							title='Текущее значение'
							render={(_, record) => (
								<TagCompactValue type={record.type} record={record.value} quality={record.value.quality} />
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
