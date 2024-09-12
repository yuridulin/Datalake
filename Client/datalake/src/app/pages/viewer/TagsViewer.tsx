import {
	Button,
	Col,
	DatePicker,
	Divider,
	Row,
	Select,
	Space,
	Switch,
	Table,
} from 'antd'

import { DefaultOptionType } from 'antd/es/select'
import Column from 'antd/es/table/Column'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { TagQuality, TagType } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import TagQualityEl from '../../components/TagQualityEl'
import TagValueEl from '../../components/TagValueEl'
import routes from '../../router/routes'

type ValueType = {
	guid: string
	name: string
	type: TagType
	value?: string | number | boolean | null
	quality: TagQuality
	date: string
}

export default function TagsViewer() {
	const [tags, setTags] = useState([] as DefaultOptionType[])
	const [values, setValues] = useState([] as ValueType[])

	const [request, setRequest] = useState({
		tags: [] as string[],
		date: dayjs(new Date()),
		isLive: true,
	})

	const loadTags = () => {
		api.tagsReadAll()
			.then((res) => {
				setTags(
					res.data.map((x) => ({
						label: x.name,
						title: x.name,
						value: x.guid,
					})),
				)
			})
			.catch(() => setTags([]))
	}

	const getValues = () => {
		if (request.tags.length === 0) return setValues([])
		api.valuesGet([
			{
				requestKey: 'viewer-tags',
				tags: request.tags,
				...(request.isLive
					? {}
					: { exact: request.date.format('YYYY-MM-DDThh:mm:ss') }),
			},
		])
			.then((res) => {
				setValues(
					res.data[0].tags
						.map((x) => {
							const valueObject = x.values[0]
							return {
								guid: x.guid,
								name: x.name,
								value: valueObject.value,
								type: x.type,
								date: valueObject.dateString,
								quality: valueObject.quality,
							}
						})
						.sort((a, b) => -1 * a.name.localeCompare(b.name)),
				)
			})
			.catch(() => setValues([]))
	}

	useEffect(loadTags, [])
	useEffect(getValues, [request])
	useInterval(() => {
		if (!request.isLive) return
		getValues()
	}, 1000)

	return (
		<>
			<Row>
				<Col flex='auto'>
					<Select
						options={tags}
						mode='tags'
						style={{ width: '100%' }}
						onChange={(e) => setRequest({ ...request, tags: e })}
					/>
				</Col>
				<Col flex='20em'>
					<Space>
						<Switch
							defaultChecked
							checkedChildren='текущие'
							unCheckedChildren='на время'
							onChange={(e) =>
								setRequest({ ...request, isLive: e })
							}
						/>
						<span
							style={{
								display: request.isLive ? 'none' : 'inherit',
							}}
						>
							<DatePicker
								onChange={(e) =>
									setRequest({ ...request, date: e })
								}
							/>
						</span>
					</Space>
				</Col>
			</Row>
			<Divider orientation='left'>Значения</Divider>
			<Table dataSource={values} size='small'>
				<Column
					title='Тег'
					key='tag'
					dataIndex='guid'
					render={(guid, row: ValueType) => (
						<NavLink to={routes.Tags.routeToTag(guid)}>
							<Button size='small'>{row.name}</Button>
						</NavLink>
					)}
				/>
				<Column
					title='Значение'
					key='value'
					dataIndex='value'
					render={(value, row: ValueType) => (
						<TagValueEl
							type={row.type}
							guid={row.guid}
							allowEdit={true}
							value={value}
						/>
					)}
				/>
				<Column
					title='Качество'
					key='quality'
					dataIndex='quality'
					render={(quality) => <TagQualityEl quality={quality} />}
				/>
			</Table>
		</>
	)
}
