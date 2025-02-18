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

import api from '@/api/swagger-api'
import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import HistoricValuesMode from '@/app/pages/values/tagViewerModes/HistoricValuesMode'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'
import { TransformedData } from '@/app/pages/values/types/TransformedData'
import {
	CheckSquareOutlined,
	CloseSquareOutlined,
	PlaySquareOutlined,
} from '@ant-design/icons'
import { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { TagQuality } from '../../../api/swagger/data-contracts'
import compareValues from '../../../functions/compareValues'
import { useInterval } from '../../../hooks/useInterval'
import { TagValue } from '../../../types/tagValue'
import TagCompactValue from '../../components/TagCompactValue'

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
	const [searchParams, setSearchParams] = useSearchParams()
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)

	// Парсинг параметров из URL при инициализации
	const initialTags = searchParams.get('tags')?.split('|').map(Number) || []
	const initialMode =
		(searchParams.get('mode') as 'live' | 'exact' | 'old-young') || 'live'

	// Функция для парсинга даты из URL
	const parseDate = (param: string | null, fallback: dayjs.Dayjs) =>
		param ? dayjs(param, timeMask) : fallback

	const [request, setRequest] = useState({
		tags: initialTags,
		old: parseDate(searchParams.get('old'), dayjs().add(-1, 'hour')),
		young: parseDate(searchParams.get('young'), dayjs()),
		exact: parseDate(searchParams.get('exact'), dayjs()),
		resolution: Number(searchParams.get('resolution')) || 0,
		mode: initialMode,
		update: false,
	})

	const [checkedTags, setCheckedTags] = useState(initialTags)
	const [values, setValues] = useState([] as TagValueWithInfo[])
	const [rangeValues, setRangeValues] = useState([] as TransformedData[])
	const [rangeColumns, setRangeColumns] = useState(
		[] as ColumnsType<TransformedData>,
	)

	// Оптимизация: мемоизация обработчиков
	const handleTagChange = useCallback(
		(value: number[], currentTagMapping: FlattenedNestedTagsType) => {
			setCheckedTags(value)
			setTagMapping(currentTagMapping)
			setRequest((prev) => ({ ...prev, tags: value }))
		},
		[],
	)

	const handleModeChange = useCallback(
		(value: 'live' | 'exact' | 'old-young') => {
			setRequest((prev) => ({ ...prev, mode: value }))
		},
		[],
	)

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
				// Обогащаем теги дополнительной информацией
				setValues(
					res.data[0].tags.map((tag) => {
						const mapping = tagMapping[tag.id]
						return {
							...tag,
							localName: mapping?.localName ?? tag.name,
						} as TagValueWithInfo
					}),
				)


			})
			.catch(() => setValues([]))
	}

	useInterval(() => {
		if (request.mode === 'live' && request.update) getValues()
	}, 1000)

	// Эффект для обновления URL при изменении состояния
	useEffect(() => {
		const params: Record<string, string> = {
			tags: checkedTags.join('|'),
			mode: request.mode,
			resolution: String(request.resolution),
		}

		if (request.mode === 'exact') {
			params.exact = request.exact.format(timeMask)
		} else if (request.mode === 'old-young') {
			params.old = request.old.format(timeMask)
			params.young = request.young.format(timeMask)
		}

		setSearchParams(params, { replace: true })
	}, [checkedTags, request, setSearchParams])

	return (
		<>
			<div style={{ position: 'sticky' }}>
				<Row>
					<QueryTreeSelect onChange={handleTagChange} />
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
							onChange={(e) => handleModeChange(e.target.value)}
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

			) : (
				<HistoricValuesMode values={values} />
			)}
		</>
	)
})

export default TagsViewer
