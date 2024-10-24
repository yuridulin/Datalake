import {
	Button,
	Col,
	DatePicker,
	Divider,
	Radio,
	Row,
	Select,
	Table,
} from 'antd'

import {
	CheckSquareOutlined,
	CloseSquareOutlined,
	PlaySquareOutlined,
} from '@ant-design/icons'
import { DefaultOptionType } from 'antd/es/select'
import { ColumnsType } from 'antd/es/table'
import Column from 'antd/es/table/Column'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import compareValues from '../../../api/extensions/compareValues'
import { TagValue } from '../../../api/models/tagValue'
import api from '../../../api/swagger-api'
import { TagQuality, TagType } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import TagCompactValue from '../../components/TagCompactValue'
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

interface TransformedData {
	time: string
	dateString: string
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	[key: string]: any // Для динамических свойств tag1, tag2 и т.д.
}

const timeMode = [
	{ label: 'Текущие', value: 'live' },
	{ label: 'Срез', value: 'exact' },
	{ label: 'Диапазон', value: 'old-young' },
]

const resolutions = [
	{ label: 'По изменению', value: 0 },
	{ label: 'Посекундно', value: 1000 },
	{ label: 'Поминутно', value: 1000 * 60 },
	{ label: 'Почасово', value: 1000 * 60 * 60 },
]

const timeMask = 'YYYY-MM-DDTHH:mm:ss'

export default function TagsViewer() {
	const [tags, setTags] = useState([] as DefaultOptionType[])
	const [values, setValues] = useState([] as ValueType[])
	const [searchValue, setSearchValue] = useState('')
	const [rangeValues, setRangeValues] = useState([] as TransformedData[])
	const [rangeColumns, setRangeColumns] = useState(
		[] as ColumnsType<TransformedData>,
	)

	const [request, setRequest] = useState({
		tags: [] as number[],
		old: dayjs(new Date())
			.add(-1, 'hour')
			.set('minute', 0)
			.set('second', 0),
		young: dayjs(new Date()).set('minute', 0).set('second', 0),
		exact: dayjs(new Date()),
		resolution: 0,
		mode: 'live' as 'live' | 'exact' | 'old-young',
		update: false,
	})

	const loadTags = () => {
		api.tagsReadAll()
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
		const timeSettings =
			request.mode === 'live'
				? {}
				: request.mode === 'exact'
				? { exact: request.exact.format(timeMask) }
				: {
						old: request.old.format(timeMask),
						young: request.young.format(timeMask),
						resolution: request.resolution,
				  }
		api.valuesGet([
			{
				requestKey: 'viewer-tags',
				tagsId: request.tags,
				...timeSettings,
			},
		])
			.then((res) => {
				// Преобразование данных
				const transformedData = res.data[0].tags.reduce(
					(
						acc: Record<string, TransformedData>,
						{ id, guid, name, type, values },
					) => {
						values.forEach(
							({ date, dateString, value, quality }) => {
								if (!acc[date])
									acc[date] = { time: dateString, dateString }
								acc[date][guid] = {
									id,
									guid,
									name,
									type,
									value,
									quality,
								}
							},
						)
						return acc
					},
					{},
				)

				const dataSource = Object.values(transformedData)
				setRangeValues(dataSource)

				// Определение столбцов
				setRangeColumns([
					{
						title: 'Время',
						dataIndex: 'time',
						key: 'time',
						sorter: (a, b) => compareValues(a.time, b.time),
						showSorterTooltip: false,
					},
					...res.data[0].tags.map((x) => ({
						title: () => (
							<NavLink to={routes.tags.toTagForm(x.guid)}>
								<Button>{x.name}</Button>
							</NavLink>
						),
						key: x.guid,
						dataIndex: x.guid,
						render: (value: {
							quality: TagQuality
							value: TagValue
						}) => (
							<TagCompactValue
								type={x.type}
								value={value?.value ?? null}
								quality={value?.quality ?? TagQuality.Bad}
							/>
						),
					})),
				])

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
	useInterval(() => {
		if (request.mode === 'live' && request.update) getValues()
	}, 1000)

	function handleSearch(value: string): void {
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
						<Button onClick={getValues}>
							<PlaySquareOutlined />
							Запрос
						</Button>
					</Col>
					<Col flex='20em'>
						<Radio.Group
							defaultValue={'live'}
							options={timeMode}
							optionType='button'
							buttonStyle='solid'
							onChange={(e) =>
								setRequest({ ...request, mode: e.target.value })
							}
						/>
					</Col>
					<Col flex='auto'>
						<span
							style={{
								display:
									request.mode === 'live'
										? 'inherit'
										: 'none',
							}}
						>
							<Button
								onClick={() =>
									setRequest({
										...request,
										update: !request.update,
									})
								}
								type={request.update ? 'primary' : 'default'}
							>
								{request.update ? (
									<CheckSquareOutlined />
								) : (
									<CloseSquareOutlined />
								)}
								непрерывное обновление
							</Button>
						</span>
						<span
							style={{
								display:
									request.mode === 'exact'
										? 'inherit'
										: 'none',
							}}
						>
							<DatePicker
								showTime
								placeholder='Дата среза'
								defaultValue={request.exact}
								onChange={(e) =>
									setRequest({ ...request, exact: e })
								}
							/>
						</span>
						<span
							style={{
								display:
									request.mode === 'old-young'
										? 'inherit'
										: 'none',
							}}
						>
							<DatePicker
								showTime
								defaultValue={request.old}
								maxDate={request.young}
								placeholder='Начальная дата'
								onChange={(e) =>
									setRequest({ ...request, old: e })
								}
							/>
							<DatePicker
								showTime
								defaultValue={request.young}
								minDate={request.old}
								placeholder='Конечная дата'
								onChange={(e) =>
									setRequest({ ...request, young: e })
								}
							/>
							<Select
								options={resolutions}
								style={{ width: '12em' }}
								defaultValue={0}
								onChange={(e) =>
									setRequest({ ...request, resolution: e })
								}
							/>
						</span>
					</Col>
				</Row>
				<Divider orientation='left'>Значения</Divider>
			</div>
			{request.mode === 'old-young' ? (
				<Table
					columns={rangeColumns}
					dataSource={rangeValues}
					size='small'
					rowKey='date'
				/>
			) : (
				<Table dataSource={values} size='small' rowKey='guid'>
					<Column
						title='Тег'
						dataIndex='guid'
						render={(guid, row: ValueType) => (
							<NavLink to={routes.tags.toTagForm(guid)}>
								<Button size='small'>{row.name}</Button>
							</NavLink>
						)}
					/>
					<Column
						width='40%'
						title='Значение'
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
						width='10em'
						title='Качество'
						dataIndex='quality'
						render={(quality) => <TagQualityEl quality={quality} />}
					/>
				</Table>
			)}
		</>
	)
}
