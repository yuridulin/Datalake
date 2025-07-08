import api from '@/api/swagger-api'
import HelpAggregationType from '@/app/components/help-tootip/help-pages/HelpAggregationType'
import HelpNCalc from '@/app/components/help-tootip/help-pages/HelpNCalc'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import TagTreeSelect from '@/app/components/tagTreeSelect/TagTreeSelect'
import getTagFrequencyName from '@/functions/getTagFrequencyName'
import { TagValue } from '@/types/tagValue'
import { AppstoreAddOutlined, DeleteOutlined } from '@ant-design/icons'
import { Button, Checkbox, Input, InputNumber, Popconfirm, Radio, Select, Space, Spin } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
	AggregationPeriod,
	BlockTreeInfo,
	SourceType,
	TagAggregation,
	TagFrequency,
	TagInfo,
	TagSimpleInfo,
	TagType,
	TagUpdateInputRequest,
	TagUpdateRequest,
} from '../../../../api/swagger/data-contracts'
import { useInterval } from '../../../../hooks/useInterval'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import TagValueEl from '../../../components/TagValueEl'
import routes from '../../../router/routes'

type SourceOption = {
	value: number
	label: string
}

enum SourceStrategy {
	Manual = SourceType.Manual,
	Calculated = SourceType.Calculated,
	Aggregated = SourceType.Aggregated,
	FromSource = 0,
}

interface UpdateRequest extends TagUpdateRequest {
	formulaInputs: UpdateInputRequest[]
}

interface UpdateInputRequest extends TagUpdateInputRequest {
	key: number
}

const TagForm = () => {
	const { id } = useParams()
	const navigate = useNavigate()

	// инфа
	const [tag, setTag] = useState({} as TagInfo)
	const [sources, setSources] = useState([] as SourceOption[])
	const [value, setValue] = useState(null as TagValue)
	const [blocks, setBlocks] = useState([] as BlockTreeInfo[])
	const [tags, setTags] = useState([] as TagSimpleInfo[])

	// изменяемые
	const [isLoading, setLoading] = useState(false)
	const [request, setRequest] = useState({
		formulaInputs: [] as UpdateInputRequest[],
	} as UpdateRequest)
	const [items, setItems] = useState([] as { value: string }[])
	const [strategy, setStrategy] = useState(SourceStrategy.Manual)

	// получение инфы
	const loadTagData = () => {
		if (!id) return
		setLoading(true)

		Promise.all([
			api
				.blocksReadAsTree()
				.then((res) => setBlocks(res.data))
				.catch(() => setBlocks([])),
			api
				.tagsReadAll()
				.then((res) => {
					setTags(res.data)
				})
				.catch(() => setTags([])),
			api.tagsRead(Number(id)).then((res) => {
				const info = res.data
				setTag(info)
				setRequest({
					...info,
					sourceTagId: info.sourceTag?.id,
					formulaInputs: info.formulaInputs.map((x, index) => ({
						key: index,
						tagId: x.id,
						variableName: x.variableName,
					})),
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
			api.sourcesReadAll().then((res) => {
				setSources(
					res.data.map((source) => ({
						value: source.id,
						label: source.name,
					})),
				)
			}),
		]).finally(() => setLoading(false))
	}

	useEffect(loadTagData, [id])

	const getItems = () => {
		if (!request.sourceId || request.sourceId <= 0) return
		api.sourcesGetItems(request.sourceId).then((res) => {
			setItems(
				res.data.map((x) => ({
					value: x.path ?? '',
				})),
			)
		})
	}

	useEffect(getItems, [request])

	const getValue = useCallback(() => {
		if (!id) return
		if (strategy !== SourceStrategy.FromSource) return
		setValue((prevValue) => {
			api
				.valuesGet([
					{
						requestKey: 'tag-current-value',
						tags: [String(id)],
					},
				])
				.then((res) => setValue(res.data[0].tags[0].values[0].value))
				.catch(() => setValue(null))
			return prevValue
		})
	}, [id, strategy])

	useEffect(getValue, [tag, getValue])
	useInterval(getValue, 1000)

	useEffect(() => {
		if (strategy === SourceStrategy.FromSource && request.sourceId < 0) {
			setRequest({
				...request,
				sourceId: SourceType.NotSet,
			})
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [strategy])

	const back = useCallback(() => {
		navigate(routes.tags.list)
	}, [navigate])

	const tagUpdate = () => {
		api.tagsUpdate(Number(id), {
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

	const tagDelete = () => api.tagsDelete(Number(id)).then(back)

	const addParam = () => {
		if (strategy !== SourceStrategy.Calculated) return
		const existsId = request.formulaInputs.filter((x) => x.key < 0).map((x) => x.key)
		const availableFakeId = existsId.length > 0 ? Math.min.apply(0, existsId) - 1 : -1
		setRequest({
			...request,
			formulaInputs: [
				...request.formulaInputs,
				{
					key: availableFakeId,
					tagId: 0,
					variableName: '',
				},
			],
		})
	}

	const removeParam = (key: number) => {
		setRequest({
			...request,
			formulaInputs: request.formulaInputs.filter((x) => x.key !== key),
		})
	}

	return isLoading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={<Button onClick={() => navigate(routes.tags.list)}>Вернуться</Button>}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить этот тег?'
							placement='bottom'
							onConfirm={tagDelete}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						&ensp;
						<Button type='primary' onClick={tagUpdate}>
							Сохранить
						</Button>
					</>
				}
			>
				Тег {tag.name}
			</PageHeader>
			<FormRow title='Имя'>
				<Input
					value={request.name}
					onChange={(e) =>
						setRequest({
							...request,
							name: e.target.value,
						})
					}
				/>
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={request.description ?? ''}
					rows={4}
					style={{ resize: 'none' }}
					onChange={(e) =>
						setRequest({
							...request,
							description: e.target.value,
						})
					}
				/>
			</FormRow>
			<FormRow title='Тип'>
				<Radio.Group
					buttonStyle='solid'
					value={request.type}
					onChange={(e) => {
						setRequest({
							...request,
							type: e.target.value,
						})
						if (strategy === SourceStrategy.Aggregated && e.target.value !== TagType.Number) {
							setStrategy(SourceStrategy.Manual)
						}
					}}
				>
					<Radio.Button value={TagType.String}>Строка</Radio.Button>
					<Radio.Button value={TagType.Number}>Число</Radio.Button>
					<Radio.Button value={TagType.Boolean}>Логическое значение</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display: request.type === TagType.Number ? 'block' : 'none',
				}}
			>
				<FormRow>
					<Checkbox
						checked={request.isScaling}
						onChange={(e) =>
							setRequest({
								...request,
								isScaling: e.target.checked,
							})
						}
					>
						Преобразование по шкале
					</Checkbox>
				</FormRow>
				<div
					style={{
						display: request.isScaling ? 'block' : 'none',
					}}
				>
					<FormRow title='Шкала реальных значений' style={{ display: 'flex' }}>
						<InputNumber
							addonBefore='Min'
							value={request.minEu}
							onChange={(v) =>
								setRequest({
									...request,
									minEu: Number(v),
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={request.maxEu}
							onChange={(v) =>
								setRequest({
									...request,
									maxEu: Number(v),
								})
							}
						/>
					</FormRow>
					<FormRow title='Шкала преобразованных значений' style={{ display: 'flex' }}>
						<InputNumber
							addonBefore='Min'
							value={request.minRaw}
							onChange={(v) =>
								setRequest({
									...request,
									minRaw: Number(v),
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={request.maxRaw}
							onChange={(v) =>
								setRequest({
									...request,
									maxRaw: Number(v),
								})
							}
						/>
					</FormRow>
				</div>
			</div>
			<FormRow title='Промежуток времени между записью значений'>
				<Radio.Group
					buttonStyle='solid'
					value={request.frequency}
					onChange={(value) => {
						console.log(value)
						setRequest({
							...request,
							frequency: value.target.value,
						})
					}}
				>
					{Object.values(TagFrequency)
						.filter((key) => !isNaN(Number(key)))
						.map((value) => (
							<Radio.Button key={value} value={value}>
								<TagFrequencyEl frequency={value as TagFrequency} />
								&emsp;
								{getTagFrequencyName(value as number)}
							</Radio.Button>
						))}
				</Radio.Group>
			</FormRow>
			<FormRow title='Способ получения'>
				<Radio.Group buttonStyle='solid' value={strategy} onChange={(e) => setStrategy(e.target.value)}>
					<Radio.Button value={SourceStrategy.Manual}>Ручной ввод</Radio.Button>
					<Radio.Button value={SourceStrategy.Calculated}>Вычисление</Radio.Button>
					<Radio.Button value={SourceStrategy.FromSource}>Из источника</Radio.Button>
					<Radio.Button value={SourceStrategy.Aggregated} disabled={request.type !== TagType.Number}>
						Агрегирование
					</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display: strategy === SourceStrategy.Calculated ? 'block' : 'none',
				}}
			>
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
							setRequest({
								...request,
								formula: e.target.value,
							})
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
								}}
							>
								<Input
									value={input.variableName}
									placeholder='Введите обозначение переменной'
									onChange={(e) =>
										setRequest({
											...request,
											formulaInputs: [
												...request.formulaInputs.map((x) =>
													x.key !== input.key
														? x
														: {
																...x,
																variableName: e.target.value,
															},
												),
											],
										})
									}
								/>
								<TagTreeSelect
									blocks={blocks}
									tags={tags}
									value={input.tagId}
									onChange={(v) =>
										setRequest({
											...request,
											formulaInputs: request.formulaInputs.map((x) =>
												x.key !== input.key
													? x
													: {
															...x,
															tagId: v,
														},
											),
										})
									}
								/>
								<Button icon={<DeleteOutlined />} onClick={() => removeParam(input.key)}></Button>
							</div>
						))}
					</FormRow>
					<Button icon={<AppstoreAddOutlined />} onClick={addParam}></Button>
				</div>
			</div>
			<div
				style={{
					display: strategy === SourceStrategy.FromSource ? 'block' : 'none',
				}}
			>
				<FormRow title='Используемый источник'>
					<Select
						showSearch
						options={[
							{
								value: SourceType.NotSet,
								label: '? не выбран',
							},
							...sources,
						]}
						value={request.sourceId}
						onChange={(value) =>
							setRequest({
								...request,
								sourceId: value,
							})
						}
						style={{ width: '100%' }}
					/>
				</FormRow>
				<div
					style={{
						display: request.sourceId === SourceType.NotSet ? 'none' : 'inherit',
					}}
				>
					<FormRow title='Путь к данным в источнике'>
						<Select
							showSearch
							value={request.sourceItem}
							options={items}
							onChange={(value) =>
								setRequest({
									...request,
									sourceItem: value,
								})
							}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
				<FormRow title='Значение'>
					<Space>
						<TagValueEl value={value} type={tag.type} />
					</Space>
				</FormRow>
			</div>
			<div
				style={{
					display: strategy === SourceStrategy.Aggregated && request.type === TagType.Number ? 'block' : 'none',
				}}
			>
				<FormRow title='Тег-источник'>
					<TagTreeSelect
						value={request.sourceTagId ?? 0}
						blocks={blocks}
						tags={tags}
						onChange={(value) => setRequest({ ...request, sourceTagId: value })}
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
						onChange={(e) => setRequest({ ...request, aggregation: e.target.value })}
					>
						<Radio.Button value={TagAggregation.Average}>Взвешенное среднее</Radio.Button>
						<Radio.Button value={TagAggregation.Sum}>Взвешенная сумма</Radio.Button>
					</Radio.Group>
				</FormRow>
				<FormRow title='Период агрегирования'>
					<Radio.Group
						buttonStyle='solid'
						value={request.aggregationPeriod}
						onChange={(e) => setRequest({ ...request, aggregationPeriod: e.target.value })}
					>
						<Radio.Button value={AggregationPeriod.Munite}>Прошедшая минута</Radio.Button>
						<Radio.Button value={AggregationPeriod.Hour}>Прошедший час</Radio.Button>
						<Radio.Button value={AggregationPeriod.Day}>Прошедшие сутки</Radio.Button>
					</Radio.Group>
				</FormRow>
			</div>
		</>
	)
}

export default TagForm
