// TagValuesViewer.tsx
import api from '@/api/swagger-api'
import { TagType, ValueRecord } from '@/api/swagger/data-contracts'
import ExactValuesMode from '@/app/pages/values/tagViewerModes/ExactValuesMode'
import TimedValuesMode from '@/app/pages/values/tagViewerModes/TimedValuesMode'
import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { useInterval } from '@/hooks/useInterval'
import { PlaySquareOutlined } from '@ant-design/icons'
import { Button, Col, DatePicker, Divider, Radio, Row, Select, Space } from 'antd'
import dayjs, { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

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

const ModeParam = 'V-M'
const ResolutionParam = 'V-R'
const ExactParam = 'V-E'
const OldParam = 'V-O'
const YoungParam = 'V-P'

const parseDate = (param: string | null, fallback: dayjs.Dayjs) => (param ? dayjs(param, timeMask) : fallback)

interface TagValuesViewerProps {
	relations: number[] // Массив ID связей
	tagMapping: Record<number, { id: number; localName: string; type: TagType }> // Маппинг отношений
	integrated?: boolean // Скрываем часть контролов и инициируем запросы сразу после изменения настроек
}

interface ViewerSettings {
	activeRelations: number[]
	old: Dayjs
	young: Dayjs
	exact: Dayjs
	resolution: number
	mode: TimeMode
	update: boolean
}

export const TagValuesViewer = observer(({ relations, tagMapping, integrated = false }: TagValuesViewerProps) => {
	const [searchParams, setSearchParams] = useSearchParams()
	const initialMode = (searchParams.get(ModeParam) as TimeMode) || TimeModes.LIVE
	const [isLoading, setLoading] = useState(false)
	const [values, setValues] = useState<{ relationId: number; value: TagValueWithInfo }[]>([])

	const [settings, setSettings] = useState<ViewerSettings>({
		activeRelations: relations,
		old: parseDate(searchParams.get(OldParam), dayjs().startOf('hour')),
		young: parseDate(searchParams.get(YoungParam), dayjs().startOf('hour').add(1, 'hour')),
		exact: parseDate(searchParams.get(TimeModes.EXACT), dayjs()),
		resolution: Number(searchParams.get(ResolutionParam)) || 0,
		mode: initialMode,
		update: integrated,
	})

	useEffect(() => {
		if (isArraysDifferent(settings.activeRelations, relations)) {
			setSettings({ ...settings, activeRelations: relations })
		}
	}, [relations, settings])

	const getValues = useCallback(() => {
		setLoading(true)
		if (settings.activeRelations.length === 0) {
			setLoading(false)
			return setValues([])
		}

		const tagIds = Array.from(new Set(settings.activeRelations.map((relId) => tagMapping[relId]?.id).filter(Boolean)))

		const timeSettings =
			settings.mode === TimeModes.LIVE
				? {}
				: settings.mode === TimeModes.EXACT
					? { exact: settings.exact.format(timeMask) }
					: {
							old: settings.old.format(timeMask),
							young: settings.young.format(timeMask),
							resolution: settings.resolution,
						}

		api
			.valuesGet([
				{
					requestKey: 'tag-values-viewer',
					tagsId: tagIds,
					...timeSettings,
				},
			])
			.then((res) => {
				const tagValuesMap = new Map<number, ValueRecord[]>()
				res.data[0].tags.forEach((tag) => {
					tagValuesMap.set(tag.id, tag.values)
				})

				const newValues = settings.activeRelations
					.filter((relId) => tagMapping[relId])
					.map((relId) => {
						const tagInfo = tagMapping[relId]
						const tagValues = tagValuesMap.get(tagInfo.id) || []

						return {
							relationId: relId,
							value: {
								...tagInfo,
								values: tagValues,
							} as TagValueWithInfo,
						}
					})

				setValues(newValues)
			})
			.catch(console.error)
			.finally(() => setLoading(false))
	}, [tagMapping, settings])

	useEffect(() => {
		if (!integrated) return
		console.log('getValuesFromInitial')
		getValues()
	}, [settings, integrated, getValues])

	useEffect(() => {
		searchParams.set(ModeParam, settings.mode)
		searchParams.set(ResolutionParam, String(settings.resolution))
		searchParams.delete(OldParam)
		searchParams.delete(YoungParam)
		searchParams.delete(ExactParam)

		if (settings.mode === TimeModes.EXACT) {
			searchParams.set(ExactParam, settings.exact.format(timeMask))
		} else if (settings.mode === TimeModes.OLD_YOUNG) {
			searchParams.set(OldParam, settings.old.format(timeMask))
			searchParams.set(YoungParam, settings.young.format(timeMask))
		}

		setSearchParams(searchParams, { replace: true })
	}, [settings, searchParams, setSearchParams])

	// Для автоматического обновления
	useInterval(() => {
		if (settings.mode === TimeModes.LIVE && settings.update) {
			console.log('getValuesFromInterval')
			getValues()
		}
	}, 1000)

	const renderFooterOld = () => (
		<Space style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 8px' }}>
			<Button type='link' size='small' onClick={() => setSettings({ ...settings, old: settings.old.startOf('day') })}>
				Убрать время
			</Button>
			<Button
				type='link'
				size='small'
				onClick={() => setSettings({ ...settings, old: settings.young.add(-1, 'hour') })}
			>
				На час назад
			</Button>
			<Button type='link' size='small' onClick={() => setSettings({ ...settings, old: settings.young.add(-1, 'day') })}>
				На сутки назад
			</Button>
			<Button type='link' size='small' onClick={() => setSettings({ ...settings, old: dayjs() })}>
				Сейчас
			</Button>
		</Space>
	)

	const renderFooterYoung = () => (
		<Space style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 8px' }}>
			<Button
				type='link'
				size='small'
				onClick={() => setSettings({ ...settings, young: settings.young.startOf('day') })}
			>
				Сбросить время
			</Button>
			<Button
				type='link'
				size='small'
				onClick={() => setSettings({ ...settings, young: dayjs().add(1, 'day').startOf('day') })}
			>
				Завтра
			</Button>
			<Button type='link' size='small' onClick={() => setSettings({ ...settings, young: dayjs() })}>
				Сейчас
			</Button>
		</Space>
	)

	return (
		<>
			<style>{`ul.ant-picker-ranges { visibility: hidden; height: 0; }`}</style>
			{!integrated ? (
				<div style={{ position: 'sticky' }}>
					<Row style={{ marginTop: '1em' }}>
						{!integrated && (
							<Col flex='10em'>
								<Button
									onClick={getValues}
									icon={<PlaySquareOutlined />}
									type='primary'
									disabled={!settings.activeRelations.length || isLoading}
									title={
										!settings.activeRelations.length ? 'Выберите хотя бы один тег' : isLoading ? 'Идет загрузка...' : ''
									}
								>
									Запрос
								</Button>
							</Col>
						)}
						<Col flex='20em'>
							<Radio.Group
								value={settings.mode}
								options={timeModeOptions}
								optionType='button'
								onChange={(e) => setSettings({ ...settings, mode: e.target.value })}
							/>
						</Col>
						<Col flex='auto'>
							<span style={{ display: settings.mode === TimeModes.EXACT ? 'inherit' : 'none' }}>
								<DatePicker
									showTime
									placeholder='Дата среза'
									value={settings.exact}
									onChange={(e) => setSettings({ ...settings, exact: e })}
								/>
							</span>
							<span style={{ display: settings.mode === TimeModes.OLD_YOUNG ? 'inherit' : 'none' }}>
								<span style={{ padding: '0 .5em' }}>c</span>
								<DatePicker
									showTime
									value={settings.old}
									maxDate={settings.young}
									placeholder='Начальная дата'
									onChange={(e) => setSettings({ ...settings, old: e })}
									allowClear={false}
									needConfirm={false}
									renderExtraFooter={renderFooterOld}
									popupClassName='no-default-footer'
								/>
								<span style={{ padding: '0 .5em' }}>по</span>
								<DatePicker
									showTime
									value={settings.young}
									minDate={settings.old}
									placeholder='Конечная дата'
									onChange={(e) => setSettings({ ...settings, young: e })}
									allowClear={false}
									needConfirm={false}
									renderExtraFooter={renderFooterYoung}
									popupClassName='no-default-footer'
								/>
								<span style={{ padding: '0 .5em' }}>как</span>
								<Select
									options={resolutionOptions}
									style={{ width: '12em' }}
									value={settings.resolution}
									onChange={(e) => setSettings({ ...settings, resolution: e })}
								/>
							</span>
						</Col>
					</Row>
					<Divider orientation='left'>Значения</Divider>
				</div>
			) : (
				<>
					<Row style={{ marginBottom: '.5em' }}>
						{!integrated && (
							<Col flex='10em'>
								<Button
									onClick={getValues}
									icon={<PlaySquareOutlined />}
									type='primary'
									disabled={!settings.activeRelations.length || isLoading}
									title={
										!settings.activeRelations.length ? 'Выберите хотя бы один тег' : isLoading ? 'Идет загрузка...' : ''
									}
								>
									Запрос
								</Button>
							</Col>
						)}
						<Col flex='20em'>
							<Radio.Group
								value={settings.mode}
								options={timeModeOptions}
								optionType='button'
								onChange={(e) => setSettings({ ...settings, mode: e.target.value })}
							/>
						</Col>
						<Col flex='auto'>
							<span style={{ display: settings.mode === TimeModes.EXACT ? 'inherit' : 'none' }}>
								<DatePicker
									showTime
									placeholder='Дата среза'
									value={settings.exact}
									onChange={(e) => setSettings({ ...settings, exact: e })}
								/>
							</span>
							<span style={{ display: settings.mode === TimeModes.OLD_YOUNG ? 'inherit' : 'none' }}>
								<span style={{ padding: '0 .5em' }}>c</span>
								<DatePicker
									showTime
									value={settings.old}
									maxDate={settings.young}
									placeholder='Начальная дата'
									onChange={(e) => setSettings({ ...settings, old: e })}
									allowClear={false}
									needConfirm={false}
									renderExtraFooter={renderFooterOld}
									popupClassName='no-default-footer'
								/>
								<span style={{ padding: '0 .5em' }}>по</span>
								<DatePicker
									showTime
									value={settings.young}
									minDate={settings.old}
									placeholder='Конечная дата'
									onChange={(e) => setSettings({ ...settings, young: e })}
									allowClear={false}
									needConfirm={false}
									renderExtraFooter={renderFooterYoung}
									popupClassName='no-default-footer'
								/>
								<span style={{ padding: '0 .5em' }}>как</span>
								<Select
									options={resolutionOptions}
									style={{ width: '12em' }}
									value={settings.resolution}
									onChange={(e) => setSettings({ ...settings, resolution: e })}
								/>
							</span>
						</Col>
					</Row>
				</>
			)}

			{values.length ? (
				settings.mode === TimeModes.OLD_YOUNG ? (
					<TimedValuesMode relations={values} />
				) : (
					<ExactValuesMode relations={values} />
				)
			) : integrated ? (
				''
			) : settings.activeRelations.length ? (
				<>Для просмотра нажмите кнопку "Запрос"</>
			) : (
				<>Для просмотра выберите теги и настройки, затем нажмите кнопку "Запрос"</>
			)}
		</>
	)
})
