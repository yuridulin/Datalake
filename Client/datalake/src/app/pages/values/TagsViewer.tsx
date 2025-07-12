import { Button, Col, DatePicker, Divider, Radio, Row, Select, Space } from 'antd'

import api from '@/api/swagger-api'
import { ValueRecord } from '@/api/swagger/data-contracts'
import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import ExactValuesMode from '@/app/pages/values/tagViewerModes/ExactValuesMode'
import TimedValuesMode from '@/app/pages/values/tagViewerModes/TimedValuesMode'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'
import { CheckSquareOutlined, CloseSquareOutlined, PlaySquareOutlined } from '@ant-design/icons'
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

const parseDate = (param: string | null, fallback: dayjs.Dayjs) => (param ? dayjs(param, timeMask) : fallback)

const TagsViewer = observer(() => {
	const [searchParams, setSearchParams] = useSearchParams()
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)
	const initialMode = (searchParams.get('mode') as TimeMode) || TimeModes.LIVE
	const [isLoading, setLoading] = useState(false)

	const [request, setRequest] = useState({
		relations: [] as number[],
		old: parseDate(searchParams.get('old'), dayjs().startOf('hour')),
		young: parseDate(searchParams.get('young'), dayjs().startOf('hour').add(1, 'hour')),
		exact: parseDate(searchParams.get(TimeModes.EXACT), dayjs()),
		resolution: Number(searchParams.get('resolution')) || 0,
		mode: initialMode,
		update: false,
	})

	// Изменяем тип значений на массив объектов с информацией о связи
	const [values, setValues] = useState<{ relationId: number; value: TagValueWithInfo }[]>([])

	const handleTagChange = useCallback((value: number[], currentTagMapping: FlattenedNestedTagsType) => {
		setTagMapping(currentTagMapping)
		setRequest((prev) => ({ ...prev, relations: value })) // Исправляем на relations
	}, [])

	const handleModeChange = useCallback((value: TimeMode) => {
		setRequest((prev) => ({ ...prev, mode: value }))
	}, [])

	const renderFooterOld = () => (
		<Space style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 8px' }}>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, old: request.old.startOf('day') })}>
				Убрать время
			</Button>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, old: request.young.add(-1, 'hour') })}>
				На час назад
			</Button>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, old: request.young.add(-1, 'day') })}>
				На сутки назад
			</Button>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, old: dayjs() })}>
				Сейчас
			</Button>
		</Space>
	)
	const renderFooterYoung = () => (
		<Space style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 8px' }}>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, young: request.young.startOf('day') })}>
				Сбросить время
			</Button>
			<Button
				type='link'
				size='small'
				onClick={() => setRequest({ ...request, young: dayjs().add(1, 'day').startOf('day') })}
			>
				Завтра
			</Button>
			<Button type='link' size='small' onClick={() => setRequest({ ...request, young: dayjs() })}>
				Сейчас
			</Button>
		</Space>
	)

	const getValues = () => {
		setLoading(true)
		if (request.relations.length === 0) {
			setLoading(false)
			return setValues([])
		}

		// Получаем уникальные tagId из выбранных связей
		const tagIds = Array.from(new Set(request.relations.map((relId) => tagMapping[relId]?.id).filter(Boolean)))

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
		api
			.valuesGet([
				{
					requestKey: 'viewer-tags',
					tagsId: tagIds,
					...timeSettings,
				},
			])
			.then((res) => {
				// Создаем маппинг tagId -> значения
				const tagValuesMap = new Map<number, ValueRecord[]>()
				res.data[0].tags.forEach((tag) => {
					tagValuesMap.set(tag.id, tag.values)
				})

				// Формируем значения для каждой связи
				const newValues: {
					relationId: number
					value: TagValueWithInfo
				}[] = []

				request.relations.forEach((relId) => {
					const relationInfo = tagMapping[relId]
					if (relationInfo) {
						const tagValues = tagValuesMap.get(relationInfo.id) || []
						newValues.push({
							relationId: relId,
							value: {
								...relationInfo,
								values: tagValues,
							} as TagValueWithInfo,
						})
					}
				})

				setValues(newValues)
			})
			.catch((e) => {
				console.error(e)
				setValues([])
			})
			.finally(() => setLoading(false))
	}

	useInterval(() => {
		if (request.mode === TimeModes.LIVE && request.update) getValues()
	}, 1000)

	useEffect(() => {
		searchParams.set('mode', request.mode)
		searchParams.set('resolution', String(request.resolution))

		if (request.mode === TimeModes.EXACT) {
			searchParams.set('exact', request.exact.format(timeMask))
			searchParams.delete('old')
			searchParams.delete('young')
		} else if (request.mode === TimeModes.OLD_YOUNG) {
			searchParams.delete('exact')
			searchParams.set('old', request.old.format(timeMask))
			searchParams.set('young', request.young.format(timeMask))
		}

		setSearchParams(searchParams, { replace: true })
	}, [request, searchParams, setSearchParams])

	return (
		<>
			<style>{`ul.ant-picker-ranges { visibility: hidden; height: 0; }`}</style>
			<div style={{ position: 'sticky' }}>
				<Row>
					<QueryTreeSelect onChange={handleTagChange} />
				</Row>
				<Row style={{ marginTop: '1em' }}>
					<Col flex='10em'>
						<Button
							onClick={getValues}
							icon={<PlaySquareOutlined />}
							type='primary'
							disabled={!request.relations.length || isLoading}
							title={!request.relations.length ? 'Выберите хотя бы один тег' : isLoading ? 'Идет загрузка...' : ''}
						>
							Запрос
						</Button>
					</Col>
					<Col flex='20em'>
						<Radio.Group
							value={request.mode}
							options={timeModeOptions}
							optionType='button'
							onChange={(e) => handleModeChange(e.target.value)}
						/>
					</Col>
					<Col flex='auto'>
						<span
							style={{
								display: request.mode === TimeModes.LIVE ? 'inherit' : 'none',
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
								icon={request.update ? <CheckSquareOutlined /> : <CloseSquareOutlined />}
							>
								непрерывное обновление
							</Button>
						</span>
						<span
							style={{
								display: request.mode === TimeModes.EXACT ? 'inherit' : 'none',
							}}
						>
							<DatePicker
								showTime
								placeholder='Дата среза'
								defaultValue={request.exact}
								onChange={(e) => setRequest({ ...request, exact: e })}
							/>
						</span>
						<span
							style={{
								display: request.mode === TimeModes.OLD_YOUNG ? 'inherit' : 'none',
							}}
						>
							<span style={{ padding: '0 .5em' }}>c</span>
							<DatePicker
								showTime
								value={request.old}
								maxDate={request.young}
								placeholder='Начальная дата'
								onChange={(e) => setRequest({ ...request, old: e })}
								allowClear={false}
								needConfirm={false}
								renderExtraFooter={renderFooterOld}
								popupClassName='no-default-footer'
							/>
							<span style={{ padding: '0 .5em' }}>по</span>
							<DatePicker
								showTime
								value={request.young}
								minDate={request.old}
								placeholder='Конечная дата'
								onChange={(e) => setRequest({ ...request, young: e })}
								allowClear={false}
								needConfirm={false}
								renderExtraFooter={renderFooterYoung}
								popupClassName='no-default-footer'
							/>
							<span style={{ padding: '0 .5em' }}>как</span>
							<Select
								options={resolutionOptions}
								style={{ width: '12em' }}
								defaultValue={0}
								onChange={(e) => setRequest({ ...request, resolution: e })}
							/>
						</span>
					</Col>
				</Row>
				<Divider orientation='left'>Значения</Divider>
			</div>
			{values.length ? (
				request.mode === TimeModes.OLD_YOUNG ? (
					<TimedValuesMode relations={values} />
				) : (
					<ExactValuesMode relations={values} />
				)
			) : request.relations.length ? (
				<>Для просмотра нажмите кнопку "Запрос"</>
			) : (
				<>Для просмотра выберите теги и настройки, затем нажмите кнопку "Запрос"</>
			)}
		</>
	)
})

export default TagsViewer
