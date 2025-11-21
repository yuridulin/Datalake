import FormRow from '@/app/components/FormRow'
import HelpAggregationType from '@/app/components/help-tootip/help-pages/HelpAggregationType'
import HelpNCalc from '@/app/components/help-tootip/help-pages/HelpNCalc'
import TagIcon from '@/app/components/icons/TagIcon'
import PageHeader from '@/app/components/PageHeader'
import TagTreeSelect from '@/app/components/tagTreeSelect/TagTreeSelect'
import routes from '@/app/router/routes'
import { TagResolutionNames } from '@/functions/getTagResolutionName'
import {
	BlockTreeInfo,
	SourceType,
	TagAggregation,
	TagInfo,
	TagResolution,
	TagSimpleInfo,
	TagType,
	TagUpdateInputRequest,
	TagUpdateRequest,
} from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { AppstoreAddOutlined, DeleteOutlined } from '@ant-design/icons'
import { Alert, Button, Checkbox, Input, InputNumber, Popconfirm, Radio, Select, Spin } from 'antd'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

type SourceOption = {
	value: number
	label: string
}

enum SourceStrategy {
	Manual = SourceType.Manual,
	Calculated = SourceType.Calculated,
	Thresholds = SourceType.Thresholds,
	Aggregated = SourceType.Aggregated,
	FromSource = 0,
}

interface UpdateRequest extends TagUpdateRequest {
	formulaInputs: UpdateInputRequest[]
}

interface UpdateInputRequest extends TagUpdateInputRequest {
	key: number
}

const thresholdRowStyle = {
	marginBottom: '.25em',
	display: 'grid',
	gridTemplateColumns: '5fr 5fr 1fr',
	gap: '8px',
}

const TagForm = () => {
	const store = useAppStore()
	const { id } = useParams()
	const navigate = useNavigate()

	// инфо
	const [tag, setTag] = useState({} as TagInfo)
	const [sources, setSources] = useState([] as SourceOption[])
	const [blocks, setBlocks] = useState([] as BlockTreeInfo[])
	const [tags, setTags] = useState([] as TagSimpleInfo[])

	// изменяемые
	const [isLoading, setLoading] = useState(false)
	const [request, setRequest] = useState({
		formulaInputs: [] as UpdateInputRequest[],
	} as UpdateRequest)
	const [items, setItems] = useState([] as { value: string }[])
	const [strategy, setStrategy] = useState(SourceStrategy.Manual)
	const hasLoadedRef = useRef(false)
	const lastIdRef = useRef<string | undefined>(id)
	const lastSourceIdRef = useRef<number | undefined>(request.sourceId)
	useEffect(() => console.log(request), [request])

	// получение инфо
	const loadTagData = () => {
		if (!id) return
		setLoading(true)

		Promise.all([
			store.api
				.inventoryBlocksGetTree()
				.then((res) => setBlocks(res.data))
				.catch(() => setBlocks([])),
			store.api
				.inventoryTagsGetAll()
				.then((res) => {
					setTags(res.data)
				})
				.catch(() => setTags([])),
			store.api.inventoryTagsGet(Number(id)).then((res) => {
				const info = res.data
				setTag(info)
				setRequest({
					...info,
					sourceTagId: info.sourceTag?.id,
					sourceTagBlockId: info.sourceTag?.blockId,
					thresholdSourceTagId: info.thresholdSourceTag?.id,
					thresholdSourceTagBlockId: info.thresholdSourceTag?.blockId,
					formulaInputs: info.formulaInputs.map(
						(x, index) =>
							({
								key: index,
								tagId: x.id,
								blockId: x.blockId,
								variableName: x.variableName,
							}) as unknown as UpdateInputRequest,
					),
					thresholds: info.thresholds ?? [],
				})
				setStrategy(
					info.sourceId == SourceType.Manual
						? SourceStrategy.Manual
						: info.sourceId == SourceType.Calculated
							? SourceStrategy.Calculated
							: info.sourceId == SourceType.Aggregated
								? SourceStrategy.Aggregated
								: SourceStrategy.FromSource,
				)
			}),
			store.api.inventorySourcesGetAll().then((res) => {
				setSources(
					res.data.map((source) => ({
						value: source.id,
						label: source.name,
					})),
				)
			}),
		]).finally(() => setLoading(false))
	}

	const getItems = useCallback(() => {
		if (!request.sourceId || request.sourceId <= 0) {
			setItems([])
			hasLoadedRef.current = false
			return
		}
		store.api.dataSourcesGetItems(request.sourceId).then((res) => {
			setItems(
				res.data.map((x) => ({
					value: x.path ?? '',
				})),
			)
		})
	}, [store.api, request.sourceId])

	useEffect(() => {
		// Если изменился id, сбрасываем флаг загрузки
		if (lastIdRef.current !== id) {
			hasLoadedRef.current = false
			lastIdRef.current = id
		}

		if (hasLoadedRef.current || !id) return
		hasLoadedRef.current = true
		loadTagData()
	}, [store.api, id])

	useEffect(() => {
		// Если изменился sourceId, сбрасываем флаг загрузки
		if (lastSourceIdRef.current !== request.sourceId) {
			lastSourceIdRef.current = request.sourceId
		}

		if (!request.sourceId || request.sourceId <= 0) {
			setItems([])
			return
		}
		getItems()
	}, [getItems, request.sourceId])

	useEffect(() => {
		if (strategy === SourceStrategy.FromSource && (request.sourceId ?? 0) < 0) {
			setRequest((prev) => ({
				...prev,
				sourceId: SourceType.Unset,
			}))
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [strategy])

	const back = useCallback(() => {
		navigate(routes.tags.list)
	}, [navigate])

	const tagUpdate = () => {
		store.api.inventoryTagsUpdate(Number(id), {
			...request,
			sourceId: strategy === SourceStrategy.FromSource ? request.sourceId : strategy,
			formulaInputs: strategy === SourceStrategy.Calculated ? request.formulaInputs : [],
			formula: strategy === SourceStrategy.Calculated ? request.formula : null,
			sourceItem: strategy === SourceStrategy.FromSource ? request.sourceItem : null,
			aggregation: strategy === SourceStrategy.Aggregated ? request.aggregation : null,
			aggregationPeriod: strategy === SourceStrategy.Aggregated ? request.aggregationPeriod : null,
			sourceTagId: strategy == SourceStrategy.Aggregated ? request.sourceTagId : null,
		})
	}

	const tagDelete = () => store.api.inventoryTagsDelete(Number(id)).then(back)

	const addParam = () => {
		if (strategy !== SourceStrategy.Calculated) return
		setRequest((prev) => {
			const existsId = prev.formulaInputs.filter((x) => x.key < 0).map((x) => x.key)
			const availableFakeId = existsId.length > 0 ? Math.min(...existsId) - 1 : -1
			return {
				...prev,
				formulaInputs: [
					...prev.formulaInputs,
					{
						key: availableFakeId,
						tagId: 0,
						blockId: null,
						variableName: '',
					} as unknown as UpdateInputRequest,
				],
			}
		})
	}

	const removeParam = (key: number) => {
		setRequest((prev) => ({
			...prev,
			formulaInputs: prev.formulaInputs.filter((x) => x.key !== key),
		}))
	}

	// ===== Валидация =====
	const editingTagId = useMemo(() => {
		const parsed = Number(id)
		return tag?.id ?? (Number.isFinite(parsed) ? parsed : 0)
	}, [tag?.id, id])

	const validation = useMemo(() => {
		const errors: string[] = []
		const warnings: string[] = []

		// self-reference checks
		if (strategy === SourceStrategy.Calculated) {
			const selfRefs = request.formulaInputs
				.filter((x) => (x.tagId ?? 0) > 0 && x.tagId === editingTagId)
				.map((x) => x.variableName || `[key ${x.key}]`)
			if (selfRefs.length > 0) {
				errors.push(`В формуле выбран текущий тег: ${selfRefs.join(', ')}`)
			}

			// duplicates (tagId + relationId), only valid selections
			const seen = new Map<string, number[]>()
			request.formulaInputs.forEach((x, idx) => {
				if ((x.tagId ?? 0) > 0) {
					const key = `${x.tagId}:${x.blockId ?? -1}`
					const arr = seen.get(key) ?? []
					arr.push(idx)
					seen.set(key, arr)
				}
			})
			const dups = [...seen.entries()].filter(([, idxs]) => idxs.length > 1)
			if (dups.length > 0) {
				const list = dups.map(([, idxs]) => `позиции ${idxs.map((i) => i + 1).join(', ')}`).join('; ')
				warnings.push(`В формуле есть повторяющиеся теги (${list})`)
			}
		}

		if (strategy === SourceStrategy.Thresholds) {
			if ((request.thresholdSourceTagId ?? 0) > 0 && request.thresholdSourceTagId === editingTagId) {
				errors.push('В настройках вычисления выбран текущий тег как источник')
			}

			// === Новая валидация уникальности thresholds и results ===
			if (Array.isArray(request.thresholds) && request.thresholds.length > 0) {
				// threshold values
				const seenThr = new Map<number, number[]>()
				request.thresholds.forEach((t, idx) => {
					const val = Number(t.threshold)
					if (!Number.isNaN(val)) {
						const arr = seenThr.get(val) ?? []
						arr.push(idx)
						seenThr.set(val, arr)
					}
				})
				const dupThr = [...seenThr.entries()].filter(([, idxs]) => idxs.length > 1)
				if (dupThr.length > 0) {
					const list = dupThr.map(([, idxs]) => `позиции ${idxs.map((i) => i + 1).join(', ')}`).join('; ')
					errors.push(`В настройках вычисления есть одинаковые пороговые значения (${list})`)
				}

				// result values
				const seenRes = new Map<number, number[]>()
				request.thresholds.forEach((t, idx) => {
					const val = Number(t.result)
					if (!Number.isNaN(val)) {
						const arr = seenRes.get(val) ?? []
						arr.push(idx)
						seenRes.set(val, arr)
					}
				})
				const dupRes = [...seenRes.entries()].filter(([, idxs]) => idxs.length > 1)
				if (dupRes.length > 0) {
					const list = dupRes.map(([, idxs]) => `позиции ${idxs.map((i) => i + 1).join(', ')}`).join('; ')
					warnings.push(`В настройках вычисления есть одинаковые результирующие значения (${list})`)
				}
			}
		}

		if (strategy === SourceStrategy.Aggregated) {
			if ((request.sourceTagId ?? 0) > 0 && request.sourceTagId === editingTagId) {
				errors.push('В агрегировании выбран текущий тег как источник')
			}
		}

		return { errors, warnings }
	}, [editingTagId, request, strategy])

	return isLoading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.tags.list)}>Вернуться</Button>]}
				right={[
					<Popconfirm
						title='Вы уверены, что хотите удалить этот тег?'
						placement='bottom'
						onConfirm={tagDelete}
						okText='Да'
						cancelText='Нет'
					>
						<Button>Удалить</Button>
					</Popconfirm>,
					<Button type='primary' onClick={tagUpdate} disabled={validation.errors.length > 0}>
						Сохранить
					</Button>,
				]}
				icon={<TagIcon type={tag.sourceType} />}
			>
				Тег {tag.name}
			</PageHeader>

			{/* Валидация: сводка */}
			<div
				style={{
					display: validation.errors.length + validation.warnings.length > 0 ? 'block' : 'none',
					marginBottom: 12,
				}}
			>
				{validation.errors.length > 0 && (
					<Alert
						type='error'
						description={
							<ul style={{ margin: 0, paddingInlineStart: 18 }}>
								{validation.errors.map((e, i) => (
									<li key={'err' + i}>{e}</li>
								))}
							</ul>
						}
						showIcon
						style={{ marginBottom: 8 }}
					/>
				)}
				{validation.warnings.length > 0 && (
					<Alert
						type='warning'
						description={
							<ul style={{ margin: 0, paddingInlineStart: 18 }}>
								{validation.warnings.map((w, i) => (
									<li key={'warn' + i}>{w}</li>
								))}
							</ul>
						}
						showIcon
					/>
				)}
			</div>

			<FormRow title='Имя'>
				<Input
					value={request.name}
					onChange={(e) =>
						setRequest((prev) => ({
							...prev,
							name: e.target.value,
						}))
					}
				/>
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={request.description ?? ''}
					rows={4}
					style={{ resize: 'none' }}
					onChange={(e) =>
						setRequest((prev) => ({
							...prev,
							description: e.target.value,
						}))
					}
				/>
			</FormRow>
			<FormRow title='Тип'>
				<Radio.Group
					buttonStyle='solid'
					value={request.type}
					onChange={(e) => {
						const nextType = e.target.value
						setRequest((prev) => ({
							...prev,
							type: nextType,
						}))
						if (strategy === SourceStrategy.Aggregated && nextType !== TagType.Number) {
							setStrategy(SourceStrategy.Manual)
						}
					}}
				>
					<Radio.Button value={TagType.String}>Строка</Radio.Button>
					<Radio.Button value={TagType.Number}>Число</Radio.Button>
					<Radio.Button value={TagType.Boolean}>Логическое значение</Radio.Button>
				</Radio.Group>
			</FormRow>

			{/* Числовые настройки */}
			<div style={{ display: request.type === TagType.Number ? 'block' : 'none' }}>
				<FormRow>
					<Checkbox
						checked={request.isScaling}
						onChange={(e) =>
							setRequest((prev) => ({
								...prev,
								isScaling: e.target.checked,
							}))
						}
					>
						Преобразование по шкале
					</Checkbox>
				</FormRow>
				<div style={{ display: request.isScaling ? 'block' : 'none' }}>
					<FormRow title='Шкала реальных значений' style={{ display: 'flex' }}>
						<InputNumber
							addonBefore='Min'
							value={request.minEu}
							onChange={(v) =>
								setRequest((prev) => ({
									...prev,
									minEu: Number(v),
								}))
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={request.maxEu}
							onChange={(v) =>
								setRequest((prev) => ({
									...prev,
									maxEu: Number(v),
								}))
							}
						/>
					</FormRow>
					<FormRow title='Шкала преобразованных значений' style={{ display: 'flex' }}>
						<InputNumber
							addonBefore='Min'
							value={request.minRaw}
							onChange={(v) =>
								setRequest((prev) => ({
									...prev,
									minRaw: Number(v),
								}))
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={request.maxRaw}
							onChange={(v) =>
								setRequest((prev) => ({
									...prev,
									maxRaw: Number(v),
								}))
							}
						/>
					</FormRow>
				</div>
			</div>

			<FormRow title='Промежуток времени между записью значений'>
				<Select
					style={{ width: '12em' }}
					value={request.resolution}
					onChange={(value) => {
						setRequest((prev) => ({
							...prev,
							resolution: value,
						}))
					}}
					options={Object.entries(TagResolutionNames).map(([value, text]) => ({
						value: Number(value),
						label: text,
					}))}
				/>
			</FormRow>

			<FormRow title='Способ получения'>
				<Radio.Group buttonStyle='solid' value={strategy} onChange={(e) => setStrategy(e.target.value)}>
					<Radio.Button value={SourceStrategy.Manual}>Ручной ввод</Radio.Button>
					<Radio.Button value={SourceStrategy.Calculated}>Вычисление</Radio.Button>
					<Radio.Button value={SourceStrategy.FromSource}>Из источника</Radio.Button>
					<Radio.Button value={SourceStrategy.Aggregated} disabled={request.type !== TagType.Number}>
						Агрегирование
					</Radio.Button>
					<Radio.Button value={SourceStrategy.Thresholds}>Сопоставление</Radio.Button>
				</Radio.Group>
			</FormRow>

			{/* Настройки расчета */}
			<div style={{ display: strategy === SourceStrategy.Calculated ? 'block' : 'none' }}>
				<FormRow
					title={
						<>
							{'Формула для вычисления'}
							<HelpNCalc />
						</>
					}
				>
					<Input
						value={request.formula ?? ''}
						onChange={(e) =>
							setRequest((prev) => ({
								...prev,
								formula: e.target.value,
							}))
						}
					/>
				</FormRow>

				<div>
					<FormRow title='Входные параметры формулы'>
						{request.formulaInputs.map((input) => (
							<div
								key={'input' + input.key}
								style={{
									marginBottom: '.25em',
									display: 'grid',
									gridTemplateColumns: '3fr 10fr 1fr',
									gap: '8px',
								}}
							>
								<Input
									value={input.variableName}
									placeholder='Введите обозначение переменной'
									onChange={(e) =>
										setRequest((prev) => ({
											...prev,
											formulaInputs: prev.formulaInputs.map((x) =>
												x.key !== input.key
													? x
													: {
															...x,
															variableName: e.target.value,
														},
											),
										}))
									}
								/>
								<TagTreeSelect
									blocks={blocks}
									tags={tags}
									value={[input.blockId, input.tagId]}
									onChange={([inputBlockId, inputTagId]) =>
										setRequest((prev) => ({
											...prev,
											formulaInputs: prev.formulaInputs.map((x) =>
												x.key !== input.key
													? x
													: {
															...x,
															tagId: inputTagId,
															blockId: inputBlockId,
														},
											),
										}))
									}
								/>
								<Button icon={<DeleteOutlined />} onClick={() => removeParam(input.key)} />
							</div>
						))}
					</FormRow>
					<Button icon={<AppstoreAddOutlined />} onClick={addParam} />
				</div>
			</div>

			<div style={{ display: strategy === SourceStrategy.Thresholds ? 'block' : 'none' }}>
				<FormRow title='Тег-источник'>
					<TagTreeSelect
						value={[request.thresholdSourceTagBlockId, request.thresholdSourceTagId ?? 0]}
						blocks={blocks}
						tags={tags}
						onChange={([thresholdSourceTagBlockId, thresholdSourceTagId]) => {
							setRequest((prev) => ({ ...prev, thresholdSourceTagId, thresholdSourceTagBlockId }))
						}}
					/>
				</FormRow>

				<div>
					<FormRow>
						<div style={thresholdRowStyle}>
							<span>Пороговые значения</span>
							<span>Результирующие значения</span>
							<span></span>
						</div>
						{request.thresholds?.map((threshold, i) => (
							<div key={i} style={thresholdRowStyle}>
								<InputNumber
									value={threshold.threshold}
									placeholder='Введите пороговое значение'
									onChange={(e) => {
										setRequest((prev) => ({
											...prev,
											thresholds: prev.thresholds?.map((x, index) => (index !== i ? x : { ...x, threshold: e ?? 0 })),
										}))
									}}
								/>
								<InputNumber
									value={threshold.result}
									placeholder='Введите результирующее значение'
									onChange={(e) => {
										setRequest((prev) => ({
											...prev,
											thresholds: prev.thresholds?.map((x, index) => (index !== i ? x : { ...x, result: e ?? 0 })),
										}))
									}}
								/>
								<Button
									icon={<DeleteOutlined />}
									onClick={() =>
										setRequest((prev) => ({
											...prev,
											thresholds:
												prev.thresholds && prev.thresholds.length > 0
													? prev.thresholds.filter((_, idx) => idx !== i)
													: prev.thresholds,
										}))
									}
								/>
							</div>
						))}
					</FormRow>
					<Button
						icon={<AppstoreAddOutlined />}
						onClick={() =>
							setRequest((prev) => ({
								...prev,
								thresholds: [...(prev.thresholds ?? []), { threshold: 0, result: 0 }],
							}))
						}
					/>
				</div>
			</div>

			{/* Настройки получения */}
			<div style={{ display: strategy === SourceStrategy.FromSource ? 'block' : 'none' }}>
				<FormRow title='Используемый источник'>
					<Select
						showSearch
						options={[
							{
								value: SourceType.Unset,
								label: '? не выбран',
							},
							...sources,
						]}
						value={request.sourceId}
						onChange={(value) =>
							setRequest((prev) => ({
								...prev,
								sourceId: value,
							}))
						}
						style={{ width: '100%' }}
					/>
				</FormRow>

				<div style={{ display: request.sourceId === SourceType.Unset ? 'none' : 'inherit' }}>
					<FormRow title='Путь к данным в источнике'>
						<Select
							showSearch
							value={request.sourceItem}
							options={items}
							onChange={(value) =>
								setRequest((prev) => ({
									...prev,
									sourceItem: value,
								}))
							}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
			</div>

			{/* Настройки агрегации */}
			<div
				style={{
					display: strategy === SourceStrategy.Aggregated && request.type === TagType.Number ? 'block' : 'none',
				}}
			>
				<FormRow title='Тег-источник'>
					<TagTreeSelect
						value={[request.sourceTagBlockId, request.sourceTagId ?? 0]}
						blocks={blocks}
						tags={tags}
						onChange={([sourceTagBlockId, sourceTagId]) => {
							setRequest((prev) => ({ ...prev, sourceTagId, sourceTagBlockId }))
						}}
					/>
				</FormRow>
				<FormRow
					title={
						<>
							{'Функция агрегирования'}
							<HelpAggregationType />
						</>
					}
				>
					<Radio.Group
						buttonStyle='solid'
						value={request.aggregation}
						onChange={(e) => setRequest((prev) => ({ ...prev, aggregation: e.target.value }))}
					>
						<Radio.Button value={TagAggregation.Average}>Взвешенное среднее</Radio.Button>
						<Radio.Button value={TagAggregation.Sum}>Взвешенная сумма</Radio.Button>
					</Radio.Group>
				</FormRow>
				<FormRow title='Период агрегирования'>
					<Radio.Group
						buttonStyle='solid'
						value={request.aggregationPeriod}
						onChange={(e) => setRequest((prev) => ({ ...prev, aggregationPeriod: e.target.value }))}
					>
						<Radio.Button value={TagResolution.Minute}>Прошедшая минута</Radio.Button>
						<Radio.Button value={TagResolution.Hour}>Прошедший час</Radio.Button>
						<Radio.Button value={TagResolution.Day}>Прошедшие сутки</Radio.Button>
					</Radio.Group>
				</FormRow>
			</div>
		</>
	)
}

export default TagForm
