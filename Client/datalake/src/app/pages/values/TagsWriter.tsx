import { Button, Col, DatePicker, Divider, Row, Select, Spin } from 'antd'

import api from '@/api/swagger-api'
import { TagQuality, TagType } from '@/api/swagger/data-contracts'
import { CustomSource } from '@/types/customSource'
import { TagValue } from '@/types/tagValue'
import { PlaySquareOutlined } from '@ant-design/icons'
import { DefaultOptionType } from 'antd/es/select'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'

type ExactValue = {
	guid: string
	name: string
	value: TagValue
	type: TagType
	quality: TagQuality
	newValue: TagValue
	hasNewValue: boolean
}

const timeMask = 'YYYY-MM-DDTHH:mm:ss'

const TagsWriter = observer(() => {
	const [tags, setTags] = useState([] as DefaultOptionType[])
	const [values, setValues] = useState([] as ExactValue[])
	const [searchValue, setSearchValue] = useState('')
	const [loading, setLoading] = useState(false)

	const [request, setRequest] = useState({
		tags: [] as number[],
		exact: dayjs(new Date()),
	})

	const loadTags = () => {
		api.tagsReadAll({ sourceId: CustomSource.Manual })
			.then((res) => {
				setTags(
					res.data.map((x) => ({
						label: x.name,
						title: x.name,
						value: x.id,
					})),
				)
			})
			.catch(() => setTags([]))
	}

	const getValues = () => {
		if (request.tags.length === 0) return setValues([])
		setLoading(true)
		api.valuesGet([
			{
				requestKey: 'viewer-tags',
				tagsId: request.tags,
				exact: request.exact.format(timeMask),
			},
		])
			.then((res) => {
				const req = res.data[0]
				setValues(
					req.tags.map((tag) => {
						const value = tag.values[0]
						return {
							guid: tag.guid,
							name: tag.name,
							type: tag.type,
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

	useEffect(loadTags, [])

	const writeValues = () => {
		api.valuesWrite(
			values
				.filter((x) => x.hasNewValue)
				.map((x) => ({
					date: request.exact.format(timeMask),
					quality: TagQuality.GoodManualWrite,
					guid: x.guid,
					value: x.newValue,
				})),
		).then(getValues)
	}

	const handleSearch = (value: string) => {
		setSearchValue(value)
	}

	const handleChange = (value: number[]) => {
		setRequest({ ...request, tags: value })
	}

	return (
		<>
			<div style={{ position: 'sticky' }}>
				<Row>
					<Select
						showSearch
						mode='multiple'
						options={tags}
						optionFilterProp='label'
						placeholder='Выберите теги'
						style={{ width: '100%' }}
						onSearch={handleSearch}
						searchValue={searchValue}
						onChange={handleChange}
					/>
				</Row>
				<Row style={{ marginTop: '1em' }}>
					<Col flex='10em'>
						<DatePicker
							showTime
							placeholder='Дата среза'
							defaultValue={request.exact}
							onChange={(e) =>
								setRequest({ ...request, exact: e })
							}
						/>
					</Col>
					<Col flex='12em'>
						<Button
							onClick={getValues}
							disabled={loading}
							icon={loading ? <Spin /> : <PlaySquareOutlined />}
						>
							Запрос
						</Button>
					</Col>
					<Col flex='auto'>
						<Button
							onClick={writeValues}
							icon={<PlaySquareOutlined />}
						>
							Запись
						</Button>
					</Col>
				</Row>
				<Divider orientation='left'>Значения</Divider>
			</div>
		</>
	)
})

export default TagsWriter
