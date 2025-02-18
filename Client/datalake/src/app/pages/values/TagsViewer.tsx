import {
	Button,
	Col,
	DatePicker,
	Divider,
	Radio,
	Row,
	Select,
	Table,
	TreeSelect,
} from 'antd'

import api from '@/api/swagger-api'
import TagButton from '@/app/components/buttons/TagButton'
import {
	CheckSquareOutlined,
	CloseSquareOutlined,
	PlaySquareOutlined,
} from '@ant-design/icons'
import { DefaultOptionType } from 'antd/es/select'
import { ColumnsType } from 'antd/es/table'
import Column from 'antd/es/table/Column'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import {
	BlockTreeInfo,
	TagQuality,
	TagSimpleInfo,
} from '../../../api/swagger/data-contracts'
import compareValues from '../../../functions/compareValues'
import { useInterval } from '../../../hooks/useInterval'
import { TagValue } from '../../../types/tagValue'
import TagCompactValue from '../../components/TagCompactValue'
import TagQualityEl from '../../components/TagQualityEl'
import TagValueEl from '../../components/TagValueEl'

type ValueType = TagSimpleInfo & {
	value?: string | number | boolean | null
	quality: TagQuality
	date: string
}

const convertToTreeSelectNodes = (
	blockTree: BlockTreeInfo[],
): DefaultOptionType[] => {
	return blockTree.map((block) => ({
		title: block.name,
		key: block.id,
		value: 0 - block.id,
		children: [
			...block.tags.map((tag) => ({
				title: tag.localName,
				key: tag.id,
				value: tag.id,
			})),
			...convertToTreeSelectNodes(block.children),
		],
	}))
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

const TagsViewer = observer(() => {
	const [tree, setTree] = useState([] as DefaultOptionType[])
	const [checkedTags, setCheckedTags] = useState([] as number[])
	const [values, setValues] = useState([] as ValueType[])
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
		api.blocksReadAsTree()
			.then((res) => {
				setTree(convertToTreeSelectNodes(res.data))
			})
			.catch(() => setTree([]))
	}

	const getValues = () => {
		if (checkedTags.length === 0) return setValues([])
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
				tagsId: checkedTags,
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
						title: () => <TagButton tag={x} />,
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
								id: x.id,
								guid: x.guid,
								name: x.name,
								type: x.type,
								frequency: x.frequency,
								sourceType: x.sourceType,
								value: valueObject.value,
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

	return (
		<>
			<div style={{ position: 'sticky' }}>
				<Row>
					<TreeSelect
						treeData={tree}
						treeCheckable={true}
						showCheckedStrategy={TreeSelect.SHOW_ALL}
						value={checkedTags}
						onChange={(value) => setCheckedTags(value)}
						placeholder='Выберите теги'
						style={{ width: '100%' }}
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
						render={(_, row: ValueType) => <TagButton tag={row} />}
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
})

export default TagsViewer
