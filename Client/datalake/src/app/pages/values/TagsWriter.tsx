import api from '@/api/swagger-api'
import { TagQuality, TagSimpleInfo, TagType } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/TagCompactValue'
import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import notify from '@/state/notifications'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { TagValue } from '@/types/tagValue'
import { PlaySquareOutlined } from '@ant-design/icons'
import { Button, Checkbox, DatePicker, Divider, Input, InputNumber, Space, Spin, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'

type ExactValue = TagSimpleInfo & {
	value: TagValue
	type: TagType
	quality: TagQuality
	newValue: TagValue
	hasNewValue: boolean
}

interface TagsWriterProps {
	integrated?: boolean
}

const timeMask = 'YYYY-MM-DDTHH:mm:ss'

const TagsWriter = observer(({ integrated = false }: TagsWriterProps) => {
	const [values, setValues] = useState<ExactValue[]>([])
	const [loading, setLoading] = useState(false)
	const [relations, setRelations] = useState<number[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>({})
	const [exactDate, setExactDate] = useState<Dayjs | null>(null)

	const getValues = useCallback(() => {
		if (relations.length === 0) return setValues([])

		const tagIds = relations.map((relId) => tagMapping[relId]?.id).filter((id) => id !== undefined) as number[]
		if (tagIds.length === 0) return setValues([])

		setLoading(true)
		api
			.valuesGet([
				{
					requestKey: CLIENT_REQUESTKEY,
					tagsId: tagIds,
					exact: exactDate ? exactDate.format(timeMask) : undefined,
				},
			])
			.then((res) => {
				const req = res.data[0]
				setValues(
					req.tags.map((tag) => {
						const value = tag.values[0]
						return {
							...tag,
							value: value.value,
							quality: value.quality,
							newValue: value.value,
							hasNewValue: false,
						}
					}),
				)
			})
			.catch(() => setValues([]))
			.finally(() => setLoading(false))
	}, [exactDate, relations, tagMapping])

	const writeValues = () => {
		const valuesToWrite = values
			.filter((x) => x.hasNewValue)
			.map((x) => ({
				date: exactDate ? exactDate.format(timeMask) : null,
				quality: TagQuality.GoodManualWrite,
				guid: x.guid,
				value: x.newValue,
			}))
		if (valuesToWrite.length === 0) return notify.warn('Нет ни одного изменения')
		api.valuesWrite(valuesToWrite).then(getValues)
	}

	const handleTagChange = (value: number[], mapping: FlattenedNestedTagsType) => {
		setRelations(value)
		setTagMapping(mapping)
	}

	const setNewValue = (guid: string, newValue: TagValue) => {
		setValues(values.map((x) => (x.guid != guid ? x : { ...x, newValue, hasNewValue: true })))
	}

	// Автоматический запрос в интегрированном режиме
	useEffect(() => {
		if (integrated && relations.length > 0) {
			getValues()
		}
	}, [integrated, relations, exactDate, getValues])

	return (
		<>
			<div style={{ position: 'sticky' }}>
				<div>
					<QueryTreeSelect onChange={handleTagChange} />
				</div>
				<div style={{ marginTop: '1em' }}>
					<Space>
						<DatePicker
							showTime
							placeholder='Выбрать момент'
							value={exactDate}
							onChange={setExactDate}
							disabled={integrated && relations.length === 0}
						/>
						{!integrated && (
							<>
								<Button
									onClick={getValues}
									disabled={loading || relations.length === 0}
									style={{ width: '9em', textAlign: 'left' }}
									icon={loading ? <Spin size='small' /> : <PlaySquareOutlined />}
								>
									Запрос
								</Button>
								<Button
									onClick={writeValues}
									icon={<PlaySquareOutlined />}
									disabled={values.filter((x) => x.hasNewValue).length === 0}
								>
									Запись
								</Button>
							</>
						)}
					</Space>
				</div>
				<Divider orientation='left'>Значения</Divider>
				{integrated && relations.length === 0 && <Divider orientation='left'></Divider>}
				{!values.length ? (
					<>Выберите теги для редактирования</>
				) : (
					<>
						<Table size='small' rowKey='guid' dataSource={values} pagination={false}>
							<Column<ExactValue>
								title='Тег'
								render={(_, record) => {
									return <TagButton tag={record} />
								}}
							/>
							<Column<ExactValue>
								title='Текущее значение'
								render={(_, record) => (
									<TagCompactValue type={record.type} value={record.value} quality={record.quality} />
								)}
							/>
							<Column<ExactValue>
								title='Новое значение'
								render={(_, record) => {
									switch (record.type) {
										case TagType.Boolean:
											return (
												<Checkbox
													checked={Boolean(record.newValue)}
													onChange={(e) => setNewValue(record.guid, e.target.checked ? 1 : 0)}
												/>
											)
										case TagType.Number:
											return (
												<InputNumber
													size='small'
													value={Number(record.newValue)}
													onChange={(e) => setNewValue(record.guid, e)}
													placeholder='Введите новое значение'
												/>
											)
										case TagType.String:
											return (
												<Input
													value={String(record.newValue)}
													onChange={(e) => setNewValue(record.guid, e.target.value)}
													placeholder='Введите новое значение'
												/>
											)
										default:
											return <></>
									}
								}}
							/>
							<Column<ExactValue>
								title='Ожидает записи'
								render={(_, record) => <Checkbox checked={record.hasNewValue} disabled />}
							/>
						</Table>
					</>
				)}
			</div>
		</>
	)
})

export default TagsWriter
