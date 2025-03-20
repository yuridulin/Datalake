import { Button, Checkbox, DatePicker, Divider, Input, InputNumber, Space, Spin, Table } from 'antd'

import api from '@/api/swagger-api'
import { TagQuality, TagSimpleInfo, TagType } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/TagCompactValue'
import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import notify from '@/state/notifications'
import { TagValue } from '@/types/tagValue'
import { PlaySquareOutlined } from '@ant-design/icons'
import Column from 'antd/es/table/Column'
import { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useState } from 'react'

type ExactValue = TagSimpleInfo & {
	value: TagValue
	type: TagType
	quality: TagQuality
	newValue: TagValue
	hasNewValue: boolean
}

const timeMask = 'YYYY-MM-DDTHH:mm:ss'

const TagsWriter = observer(() => {
	const [values, setValues] = useState([] as ExactValue[])
	const [loading, setLoading] = useState(false)

	const [request, setRequest] = useState({
		tags: [] as number[],
		exact: null as Dayjs | null,
	})

	const getValues = () => {
		if (request.tags.length === 0) return setValues([])
		setLoading(true)
		api
			.valuesGet([
				{
					requestKey: 'tags-writer',
					tagsId: request.tags,
					exact: request.exact ? request.exact.format(timeMask) : undefined,
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
	}

	const writeValues = () => {
		const valuesToWrite = values
			.filter((x) => x.hasNewValue)
			.map((x) => ({
				date: request.exact ? request.exact.format(timeMask) : null,
				quality: TagQuality.GoodManualWrite,
				guid: x.guid,
				value: x.newValue,
			}))
		if (valuesToWrite.length === 0) return notify.warn('Нет ни одного изменения')
		api.valuesWrite(valuesToWrite).then(getValues)
	}

	const handleChange = (value: number[]) => {
		setRequest({ ...request, tags: value })
	}

	const setNewValue = (guid: string, newValue: TagValue) => {
		setValues(values.map((x) => (x.guid != guid ? x : { ...x, newValue: newValue, hasNewValue: true })))
	}

	return (
		<>
			<div style={{ position: 'sticky' }}>
				<div>
					<QueryTreeSelect onChange={handleChange} />
				</div>
				<div style={{ marginTop: '1em' }}>
					<Space>
						<DatePicker
							showTime
							placeholder='Выбрать момент'
							defaultValue={request.exact}
							onChange={(e) => setRequest({ ...request, exact: e })}
						/>
						<Button
							onClick={getValues}
							disabled={loading}
							style={{ width: '9em', textAlign: 'left' }}
							icon={loading ? <Spin /> : <PlaySquareOutlined />}
						>
							Запрос
						</Button>
						<Button onClick={writeValues} icon={<PlaySquareOutlined />}>
							Запись
						</Button>
					</Space>
				</div>
				<Divider orientation='left'>
					<small>Значения</small>
				</Divider>
				{values.length > 0 && (
					<Table size='small' rowKey='guid' dataSource={values}>
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
				)}
			</div>
		</>
	)
})

export default TagsWriter
