import PollingLoader from '@/app/components/loaders/PollingLoader'
import { ExcelExportModeHandles } from '@/app/components/values/functions/exportExcel'
import ExactValuesMode from '@/app/components/values/modes/ExactValuesMode'
import TimedValuesMode from '@/app/components/values/modes/TimedValuesMode'
import { TagValueWithInfo } from '@/app/router/pages/values/types/TagValueWithInfo'
import routes from '@/app/router/routes'
import { TagResolutionNames } from '@/functions/getTagResolutionName'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { serializeDate, serializeTags, setViewerParams, TimeMode, TimeModes, URL_PARAMS } from '@/functions/urlParams'
import { SourceType, TagResolution, TagType, ValueRecord } from '@/generated/data-contracts'
import { timeMask } from '@/store/appStore'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { PlaySquareOutlined } from '@ant-design/icons'
import { Button, Col, DatePicker, Divider, Radio, Row, Select, Space, Typography } from 'antd'
import dayjs, { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'

const timeModeOptions: { label: string; value: TimeMode }[] = [
	{ label: 'Текущие', value: TimeModes.LIVE },
	{ label: 'Срез', value: TimeModes.EXACT },
	{ label: 'Диапазон', value: TimeModes.OLD_YOUNG },
]

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

const TagsValuesViewer = observer(({ relations, tagMapping, integrated = false }: TagValuesViewerProps) => {
	const store = useAppStore()
	const navigate = useNavigate()
	const [searchParams, setSearchParams] = useSearchParams()
	const initialMode = (searchParams.get(URL_PARAMS.VIEWER_MODE) as TimeMode) || TimeModes.LIVE
	const [isLoading, setLoading] = useState(false)
	const [values, setValues] = useState<{ relationId: number; value: TagValueWithInfo }[]>([])
	const [showWrite, setWrite] = useState<boolean>(false)

	const tableRef = useRef<ExcelExportModeHandles>(null)

	const [settings, setSettings] = useState<ViewerSettings>({
		activeRelations: relations,
		old: parseDate(searchParams.get(URL_PARAMS.VIEWER_OLD), dayjs().startOf('hour')),
		young: parseDate(searchParams.get(URL_PARAMS.VIEWER_YOUNG), dayjs().startOf('hour').add(1, 'hour')),
		exact: parseDate(searchParams.get(TimeModes.EXACT), dayjs()),
		resolution: Number(searchParams.get(URL_PARAMS.VIEWER_RESOLUTION)) || 0,
		mode: initialMode,
		update: integrated,
	})

	// последние настройки на момент успешного запроса
	const [lastFetchSettings, setLastFetchSettings] = useState<ViewerSettings>(settings)

	// Проверка, что настройки изменились
	const isDirty =
		settings.mode !== lastFetchSettings.mode ||
		settings.resolution !== lastFetchSettings.resolution ||
		!settings.exact.isSame(lastFetchSettings.exact) ||
		!settings.old.isSame(lastFetchSettings.old) ||
		!settings.young.isSame(lastFetchSettings.young) ||
		isArraysDifferent(settings.activeRelations, lastFetchSettings.activeRelations)

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

		return store.api
			.valuesGet([
				{
					requestKey: CLIENT_REQUESTKEY,
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
				setLastFetchSettings(settings)
				setWrite(res.data[0].tags.some((x) => x.sourceType === SourceType.Manual))
			})
			.catch(console.error)
			.finally(() => setLoading(false))
	}, [store.api, tagMapping, settings])

	useEffect(() => {
		if (!integrated) return
		getValues()
	}, [settings, integrated, getValues])

	useEffect(() => {
		setViewerParams(searchParams, {
			mode: settings.mode,
			resolution: settings.resolution,
			exact: settings.exact,
			old: settings.old,
			young: settings.young,
		})
		setSearchParams(
			(prev) => {
				console.log('TagsValuesViewer set search!')
				console.log('prev:', prev)
				console.log('next:', searchParams)
				return searchParams
			},
			{ replace: true },
		)
	}, [settings, searchParams, setSearchParams])

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

	const DateRange = () => (
		<>
			{values.length ? (
				<Col flex='8em'>
					<Button onClick={() => tableRef.current?.exportToExcel()}>Экспорт</Button>
				</Col>
			) : (
				<></>
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
						style={{ width: '13em' }}
						placeholder='Дата среза'
						value={settings.exact}
						allowClear={false}
						needConfirm={false}
						onChange={(e) => setSettings({ ...settings, exact: e })}
					/>
				</span>
				<span style={{ display: settings.mode === TimeModes.OLD_YOUNG ? 'inherit' : 'none' }}>
					<span style={{ padding: '0 .5em' }}>c</span>
					<DatePicker
						showTime
						style={{ width: '13em' }}
						value={settings.old}
						maxDate={settings.young}
						placeholder='Начальная дата'
						onChange={(e) => setSettings({ ...settings, old: e })}
						allowClear={false}
						needConfirm={false}
						renderExtraFooter={renderFooterOld}
						classNames={{ popup: { root: 'no-default-footer' } }}
					/>
					<span style={{ padding: '0 .5em' }}>по</span>
					<DatePicker
						showTime
						style={{ width: '13em' }}
						value={settings.young}
						minDate={settings.old}
						placeholder='Конечная дата'
						onChange={(e) => setSettings({ ...settings, young: e })}
						allowClear={false}
						needConfirm={false}
						renderExtraFooter={renderFooterYoung}
						classNames={{ popup: { root: 'no-default-footer' } }}
					/>
					<span style={{ padding: '0 .5em' }}>шаг</span>
					<Select
						options={Object.entries(TagResolutionNames).map((x) => ({ label: x[1], value: Number(x[0]) }))}
						style={{ width: '12em' }}
						value={settings.resolution}
						onChange={(e) => setSettings({ ...settings, resolution: e })}
					/>
				</span>
			</Col>

			{/* Компактное предупреждение об устаревших данных */}
			{isDirty && !integrated && (
				<Col flex='10em'>
					<Typography.Text type='warning' style={{ marginLeft: '1em' }}>
						Данные устарели
					</Typography.Text>
				</Col>
			)}
		</>
	)

	const handleWriteClick = () => {
		let writeDate: Dayjs
		switch (settings.mode) {
			case TimeModes.LIVE:
				writeDate = dayjs()
				break
			case TimeModes.EXACT:
				writeDate = settings.exact
				break
			case TimeModes.OLD_YOUNG:
				writeDate = settings.young
				break
			default:
				writeDate = dayjs()
		}

		const params = new URLSearchParams({
			[URL_PARAMS.TAGS]: serializeTags(settings.activeRelations, tagMapping),
			[URL_PARAMS.WRITER_DATE]: serializeDate(writeDate)!,
		})

		navigate(routes.values.tagsWriter + `?${params.toString()}`)
	}

	return (
		<>
			{/* Панель управления: кнопка «Запрос», Radio, DatePicker, Select */}
			<style>{`ul.ant-picker-ranges { visibility: hidden; height: 0; }`}</style>
			{!integrated ? (
				<div style={{ position: 'sticky' }}>
					<Row style={{ marginTop: '1em' }}>
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
						<DateRange />
						{showWrite && (
							<Col>
								<Button onClick={handleWriteClick}>Перейти к записи</Button>
							</Col>
						)}
					</Row>
					<Divider orientation='left'>Значения</Divider>
				</div>
			) : (
				<>
					<Row style={{ marginBottom: '1em' }}>
						<DateRange />
						{showWrite && (
							<Col>
								<Button onClick={handleWriteClick}>Перейти к записи</Button>
							</Col>
						)}
					</Row>
				</>
			)}

			{settings.mode === TimeModes.LIVE && settings.activeRelations.length > 0 && (
				<PollingLoader pollingFunction={getValues} interval={5000} statusDuration={400} />
			)}
			{values.length ? (
				settings.mode === TimeModes.OLD_YOUNG ? (
					<TimedValuesMode ref={tableRef} relations={values} locf={settings.resolution === TagResolution.NotSet} />
				) : (
					<ExactValuesMode ref={tableRef} relations={values} />
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

export default TagsValuesViewer
