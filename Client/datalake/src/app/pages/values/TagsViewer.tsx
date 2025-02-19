import { Button, Col, DatePicker, Divider, Radio, Row, Select } from 'antd'

import api from '@/api/swagger-api'
import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import ExactValuesMode from '@/app/pages/values/tagViewerModes/ExactValuesMode'
import TimedValuesMode from '@/app/pages/values/tagViewerModes/TimedValuesMode'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'
import {
	CheckSquareOutlined,
	CloseSquareOutlined,
	PlaySquareOutlined,
} from '@ant-design/icons'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useInterval } from '../../../hooks/useInterval'

const TimeModes = {
	LIVE: 'live',
	EXACT: 'exact',
	OLD_YOUNG: 'old-young',
} as const

type TimeMode = (typeof TimeModes)[keyof typeof TimeModes]

const timeModeOptions: { label: string; value: TimeMode }[] = [
	{ label: 'Текущие', value: TimeModes.LIVE },
	{ label: 'Срез', value: TimeModes.EXACT },
	{ label: 'Диапазон', value: TimeModes.OLD_YOUNG },
]

const resolutionOptions = [
	{ label: 'По изменению', value: 0 },
	{ label: 'Посекундно', value: 1000 },
	{ label: 'Поминутно', value: 1000 * 60 },
	{ label: 'Почасово', value: 1000 * 60 * 60 },
]

const timeMask = 'YYYY-MM-DDTHH:mm:ss'

const parseDate = (param: string | null, fallback: dayjs.Dayjs) =>
	param ? dayjs(param, timeMask) : fallback

const TagsViewer = observer(() => {
	const [searchParams, setSearchParams] = useSearchParams()
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)

	const initialTags = searchParams.get('tags')?.split('|').map(Number) || []
	const initialMode = (searchParams.get('mode') as TimeMode) || TimeModes.LIVE

	const [request, setRequest] = useState({
		tags: initialTags,
		old: parseDate(searchParams.get('old'), dayjs().add(-1, 'hour')),
		young: parseDate(searchParams.get('young'), dayjs()),
		exact: parseDate(searchParams.get(TimeModes.EXACT), dayjs()),
		resolution: Number(searchParams.get('resolution')) || 0,
		mode: initialMode,
		update: false,
	})

	const [checkedTags, setCheckedTags] = useState(initialTags)
	const [values, setValues] = useState([] as TagValueWithInfo[])

	const handleTagChange = useCallback(
		(value: number[], currentTagMapping: FlattenedNestedTagsType) => {
			setCheckedTags(value)
			setTagMapping(currentTagMapping)
			setRequest((prev) => ({ ...prev, tags: value }))
		},
		[],
	)

	const handleModeChange = useCallback((value: TimeMode) => {
		setRequest((prev) => ({ ...prev, mode: value }))
	}, [])

	const getValues = () => {
		if (checkedTags.length === 0) return setValues([])
		const timeSettings =
			request.mode === TimeModes.LIVE
				? {}
				: request.mode === TimeModes.EXACT
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
		if (request.mode === TimeModes.LIVE && request.update) getValues()
	}, 1000)

	useEffect(() => {
		const params: Record<string, string> = {
			tags: checkedTags.join('|'),
			mode: request.mode,
			resolution: String(request.resolution),
		}

		if (request.mode === TimeModes.EXACT) {
			params.exact = request.exact.format(timeMask)
		} else if (request.mode === TimeModes.OLD_YOUNG) {
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
							defaultValue={TimeModes.LIVE}
							options={timeModeOptions}
							optionType='button'
							buttonStyle='solid'
							onChange={(e) => handleModeChange(e.target.value)}
						/>
					</Col>
					<Col flex='auto'>
						<span
							style={{
								display:
									request.mode === TimeModes.LIVE
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
									request.mode === TimeModes.EXACT
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
									request.mode === TimeModes.OLD_YOUNG
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
								options={resolutionOptions}
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
			{request.mode === TimeModes.OLD_YOUNG ? (
				<TimedValuesMode values={values} />
			) : (
				<ExactValuesMode values={values} />
			)}
		</>
	)
})

export default TagsViewer
