import PollingLoader from '@/app/components/loaders/PollingLoader'
import { TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import { ExcelExportModeHandles } from '@/app/components/values/functions/exportExcel'
import ExactValuesMode from '@/app/components/values/modes/ExactValuesMode'
import TimedValuesMode from '@/app/components/values/modes/TimedValuesMode'
import { TagValueWithInfo } from '@/app/router/pages/values/types/TagValueWithInfo'
import routes from '@/app/router/routes'
import { deserializeDate, serializeDate } from '@/functions/dateHandle'
import { TagResolutionNames } from '@/functions/getTagResolutionName'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { SELECTED_SEPARATOR, setViewerParams, TimeMode, TimeModes, URL_PARAMS } from '@/functions/urlParams'
import { SourceType, TagResolution, ValueRecord } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { PlaySquareOutlined } from '@ant-design/icons'
import { Button, Col, DatePicker, Divider, Radio, Row, Select, Space, Typography } from 'antd'
import dayjs, { Dayjs } from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'

const timeModeOptions: { label: string; value: TimeMode }[] = [
	{ label: 'Текущие', value: TimeModes.LIVE },
	{ label: 'Срез', value: TimeModes.EXACT },
	{ label: 'Диапазон', value: TimeModes.OLD_YOUNG },
]

interface TagValuesViewerProps {
	relations: string[] // Массив ID связей
	tagMapping: TagMappingType // Маппинг отношений
	integrated?: boolean // Скрываем часть контролов и инициируем запросы сразу после изменения настроек
	onChange?: (settings: ViewerSettings) => void
}

export interface ViewerSettings {
	activeRelations: string[]
	old: Dayjs
	young: Dayjs
	exact: Dayjs
	resolution: TagResolution
	mode: TimeMode
	update: boolean
}

const TagsValuesViewer = observer(({ relations, tagMapping, integrated = false, onChange }: TagValuesViewerProps) => {
	const store = useAppStore()
	const navigate = useNavigate()
	const [searchParams, setSearchParams] = useSearchParams()
	const initialMode = (searchParams.get(URL_PARAMS.VIEWER_MODE) as TimeMode) || TimeModes.LIVE
	const [showWrite, setWrite] = useState<boolean>(false)

	const tableRef = useRef<ExcelExportModeHandles>(null)

	const [settings, setSettings] = useState<ViewerSettings>({
		activeRelations: relations,
		old: deserializeDate(searchParams.get(URL_PARAMS.VIEWER_OLD), dayjs().startOf('hour')),
		young: deserializeDate(searchParams.get(URL_PARAMS.VIEWER_YOUNG), dayjs().startOf('hour').add(1, 'hour')),
		exact: deserializeDate(searchParams.get(TimeModes.EXACT), dayjs()),
		resolution: Number(searchParams.get(URL_PARAMS.VIEWER_RESOLUTION)) || 0,
		mode: initialMode,
		update: integrated,
	})

	// когда мы получили настройки по умолчанию, мы отдаем им наружу
	if (onChange) onChange(settings)

	// последние настройки на момент успешного запроса
	// Инициализируем как null, чтобы при первом рендере isDirty был true
	const [lastFetchSettings, setLastFetchSettings] = useState<ViewerSettings | null>(null)

	const isTimeDirty = useMemo(
		() =>
			!lastFetchSettings ||
			settings.mode !== lastFetchSettings.mode ||
			settings.resolution !== lastFetchSettings.resolution ||
			!settings.exact.isSame(lastFetchSettings.exact) ||
			!settings.old.isSame(lastFetchSettings.old) ||
			!settings.young.isSame(lastFetchSettings.young),
		[settings, lastFetchSettings],
	)

	// Проверка, что настройки изменились
	const isDirty = useMemo(
		() => !lastFetchSettings || isTimeDirty || isArraysDifferent(settings.activeRelations, lastFetchSettings.activeRelations),
		[settings, lastFetchSettings, isTimeDirty],
	)

	useEffect(() => {
		if (isArraysDifferent(settings.activeRelations, relations)) {
			setSettings((prev) => ({ ...prev, activeRelations: relations }))
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [relations])

	// Формируем запрос для получения значений
	const valuesRequest = useMemo(() => {
		if (settings.activeRelations.length === 0) return null

		const tagIds = Array.from(
			new Set(
				settings.activeRelations
					.map((relId) => {
						const tagInfo = tagMapping[relId]
						return tagInfo?.tag?.id ?? tagInfo?.tagId
					})
					.filter((id): id is number => id !== null && id !== undefined),
			),
		)

		if (tagIds.length === 0) return null

		const timeSettings =
			settings.mode === TimeModes.LIVE
				? {}
				: settings.mode === TimeModes.EXACT
					? { exact: serializeDate(settings.exact) }
					: {
							old: serializeDate(settings.old),
							young: serializeDate(settings.young),
							resolution: settings.resolution,
						}

		return [
			{
				requestKey: CLIENT_REQUESTKEY,
				tagsId: tagIds,
				...timeSettings,
			},
		]
	}, [settings, tagMapping])

	// Получаем значения из store (реактивно через MobX)
	const valuesResponse = valuesRequest ? store.valuesStore.getValues(valuesRequest) : []

	// Проверяем состояние загрузки
	const isLoading = valuesRequest ? store.valuesStore.isLoadingValues(valuesRequest) : false

	// Преобразуем данные из store в формат компонента
	const values = useMemo(() => {
		if (valuesResponse.length === 0 || settings.activeRelations.length === 0) {
			return []
		}

		const tagValuesMap = new Map<number, ValueRecord[]>()
		valuesResponse[0]?.tags.forEach((tag) => {
			tagValuesMap.set(tag.id, tag.values)
		})

		return settings.activeRelations
			.filter((relId) => tagMapping[relId])
			.map((relId) => {
				const tagInfo = tagMapping[relId]
				const tagId = tagInfo.tag?.id ?? tagInfo.tagId ?? 0
				const tagValues = tagValuesMap.get(tagId) || []

				return {
					relationId: relId,
					value: {
						...tagInfo,
						values: tagValues,
					} as TagValueWithInfo,
				}
			})
	}, [valuesResponse, settings.activeRelations, tagMapping])

	// Обновляем showWrite на основе ответа
	useEffect(() => {
		if (valuesResponse.length > 0 && valuesResponse[0]?.tags) {
			const hasManual = valuesResponse[0].tags.some((x) => x.sourceType === SourceType.Manual)
			setWrite(hasManual)
		}
	}, [valuesResponse])

	// Функция для принудительного обновления значений (для polling и ручного запроса)
	const refreshValues = useCallback(async () => {
		if (!valuesRequest) return
		await store.valuesStore.refreshValues(valuesRequest)
		setLastFetchSettings(settings)
	}, [store.valuesStore, valuesRequest, settings])

	// Автоматически обновляем значения при изменении настроек в integrated режиме
	useEffect(() => {
		if (!integrated || !valuesRequest) return
		// При первом монтировании или при изменении настроек принудительно загружаем данные
		const shouldRefresh = !lastFetchSettings || isDirty
		if (shouldRefresh) {
			store.valuesStore.refreshValues(valuesRequest).then(() => {
				setLastFetchSettings(settings)
			}).catch(console.error)
		}
	}, [integrated, valuesRequest, isDirty, lastFetchSettings, settings, store.valuesStore])

	useEffect(() => {
		if (!isTimeDirty) return
		setSearchParams(
			(prev) => {
				setViewerParams(prev, {
					mode: settings.mode,
					resolution: settings.resolution,
					exact: settings.exact,
					old: settings.old,
					young: settings.young,
				})
				return prev
			},
			{ replace: true },
		)
		if (onChange) onChange(settings)
	}, [settings, setSearchParams, isTimeDirty, onChange])

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
			[URL_PARAMS.TAGS]: settings.activeRelations.join(SELECTED_SEPARATOR),
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
								onClick={refreshValues}
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

			{settings.mode === TimeModes.LIVE && settings.activeRelations.length > 0 && valuesRequest && (
				<PollingLoader pollingFunction={refreshValues} interval={5000} statusDuration={400} />
			)}
			{values.length ? (
				settings.mode === TimeModes.OLD_YOUNG ? (
					<TimedValuesMode ref={tableRef} relations={values} locf={settings.resolution === TagResolution.None} />
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
