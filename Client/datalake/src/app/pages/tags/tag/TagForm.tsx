import api from '@/api/swagger-api'
import { AppstoreAddOutlined } from '@ant-design/icons'
import {
	AutoComplete,
	Button,
	Checkbox,
	Input,
	InputNumber,
	Popconfirm,
	Radio,
	Select,
	Space,
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
	TagType,
	TagUpdateRequest,
} from '../../../../api/swagger/data-contracts'
import { useInterval } from '../../../../hooks/useInterval'
import { CustomSource } from '../../../../types/customSource'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import TagValueEl from '../../../components/TagValueEl'
import routes from '../../../router/routes'

const TagForm = () => {
	//#region Данные

	const { id } = useParams()
	const navigate = useNavigate()

	const [model, setModel] = useState({
		tag: {} as TagUpdateRequest,
		sourceSwitcher: CustomSource.NotSet,
		sourceId: CustomSource.NotSet,
		oldName: '',
	})

	const [sources, setSources] = useState(
		[] as { value: number; label: string }[],
	)
	const [items, setItems] = useState([] as { value: string }[])

	function getItems() {
		if (model.tag.sourceId <= 0) return
		api.sourcesGetItems(model.tag.sourceId).then((res) => {
			setItems(
				res.data.map((x) => ({
					value: x.path ?? '',
				})),
			)
		})
	}

	function load() {
		if (!id) return
		api.tagsRead(String(id)).then((res) => {
			setModel({
				...model,
				tag: res.data,
				oldName: res.data.name,
				sourceId: res.data.sourceId,
				sourceSwitcher:
					res.data.sourceId < 0
						? res.data.sourceId
						: CustomSource.NotSet,
			})
		})
		api.sourcesReadAll().then((res) => {
			setSources(
				res.data.map((source) => ({
					value: source.id,
					label: source.name,
				})),
			)
		})
		//api.tagsPossibleInputs().then((res) =>
		//setItems(
		//res.data.map((tag) => ({ value: tag.id, label: tag.name })),
		//),
		//)
	}

	const [currentValue, setCurrentValue] = useState(
		null as string | number | boolean | null,
	)
	const getCurrentValue = useCallback(() => {
		if (!id) return
		setCurrentValue((prevValue) => {
			api.valuesGet([
				{
					requestKey: 'tag-current-value',
					tags: [id],
				},
			])
				.then((res) =>
					setCurrentValue(res.data[0].tags[0].values[0].value),
				)
				.catch(() => setCurrentValue(null))
			return prevValue
		})
	}, [id])

	useInterval(() => {
		if (!id) return
		getCurrentValue()
	}, 1000)

	useEffect(() => {
		if (id) load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	useEffect(() => {
		if (model.tag.sourceId > 0) getItems()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [model.tag.sourceId])

	//#endregion

	//#region Действия

	const back = () => {
		navigate(routes.tags.list)
	}

	function tagUpdate() {
		api.tagsUpdate(String(id), model.tag).then(back)
	}

	function tagDelete() {
		api.tagsDelete(String(id)).then(back)
	}

	const addParam = () => {
		/* if (!model.tag.calcInfo) return
		let i = 1
		let exist = false
		let varName = 'x1'
		do {
			varName = 'x' + i++
			exist =
				// eslint-disable-next-line no-loop-func
				Object.keys(model.tag.calcInfo.inputs).filter(
					(x) => x === varName,
				).length > 0
		} while (exist)
		let newInputs = model.tag.calcInfo.inputs
		newInputs[varName] = 0
		setModel({
			...model,
			tag: {
				...model.tag,
				calcInfo: { ...model.tag.calcInfo, inputs: newInputs },
			},
		}) */
	}

	/* const removeParam = (param: string) => {
		let newInputs = tag.calcInfo.inputs
		delete newInputs[param]
		setTag({ ...tag, calcInfo: { ...tag.calcInfo, inputs: newInputs } })
	} */

	//#endregion

	return (
		<>
			<PageHeader
				left={
					<Button onClick={() => navigate(routes.tags.list)}>
						Вернуться
					</Button>
				}
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
				Тег {model.oldName}
			</PageHeader>
			<FormRow title='Имя'>
				<Input
					value={model.tag.name}
					onChange={(e) =>
						setModel({
							...model,
							tag: { ...model.tag, name: e.target.value },
						})
					}
				/>
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={model.tag.description ?? ''}
					rows={4}
					style={{ resize: 'none' }}
					onChange={(e) =>
						setModel({
							...model,
							tag: { ...model.tag, description: e.target.value },
						})
					}
				/>
			</FormRow>
			<FormRow title='Тип'>
				<Radio.Group
					buttonStyle='solid'
					value={model.tag.type}
					onChange={(e) =>
						setModel({
							...model,
							tag: { ...model.tag, type: e.target.value },
						})
					}
				>
					<Radio.Button value={TagType.String}>Строка</Radio.Button>
					<Radio.Button value={TagType.Number}>Число</Radio.Button>
					<Radio.Button value={TagType.Boolean}>Дискрет</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display:
						model.tag.type === TagType.Number ? 'block' : 'none',
				}}
			>
				<FormRow>
					<Checkbox
						checked={model.tag.isScaling}
						onChange={(e) =>
							setModel({
								...model,
								tag: {
									...model.tag,
									isScaling: e.target.checked,
								},
							})
						}
					>
						Преобразование по шкалам
					</Checkbox>
				</FormRow>
				<div
					style={{
						display: model.tag.isScaling ? 'block' : 'none',
					}}
				>
					<FormRow
						title='Шкала реальных значений'
						style={{ display: 'flex' }}
					>
						<InputNumber
							addonBefore='Min'
							value={model.tag.minEu}
							onChange={(v) =>
								setModel({
									...model,
									tag: {
										...model.tag,
										minEu: Number(v),
									},
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={model.tag.maxEu}
							onChange={(v) =>
								setModel({
									...model,
									tag: {
										...model.tag,
										maxEu: Number(v),
									},
								})
							}
						/>
					</FormRow>
					<FormRow
						title='Шкала преобразованных значений'
						style={{ display: 'flex' }}
					>
						<InputNumber
							addonBefore='Min'
							value={model.tag.minRaw}
							onChange={(v) =>
								setModel({
									...model,
									tag: {
										...model.tag,
										minRaw: Number(v),
									},
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={model.tag.maxRaw}
							onChange={(v) =>
								setModel({
									...model,
									tag: {
										...model.tag,
										maxRaw: Number(v),
									},
								})
							}
						/>
					</FormRow>
				</div>
			</div>
			<FormRow title='Способ получения'>
				<Radio.Group
					buttonStyle='solid'
					value={model.sourceSwitcher}
					onChange={(e) =>
						setModel({
							...model,
							sourceSwitcher: e.target.value,
							tag: {
								...model.tag,
								sourceId: e.target.value,
							},
						})
					}
				>
					<Radio.Button value={CustomSource.Manual}>
						Мануальный
					</Radio.Button>
					<Radio.Button value={CustomSource.Calculated}>
						Вычисляемый
					</Radio.Button>
					<Radio.Button value={CustomSource.NotSet}>
						Из источника
					</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display:
						model.sourceSwitcher !== CustomSource.Manual
							? 'block'
							: 'none',
				}}
			>
				<FormRow title='Интервал обновления в секундах (0, если только по изменению)'>
					<InputNumber
						value={model.tag.intervalInSeconds}
						onChange={(value) =>
							setModel({
								...model,
								tag: {
									...model.tag,
									intervalInSeconds: Number(value),
								},
							})
						}
					/>
				</FormRow>
			</div>
			<div
				style={{
					display:
						model.sourceSwitcher === CustomSource.Calculated
							? 'block'
							: 'none',
				}}
			>
				<FormRow title='Формула для вычисления'>
					<Input
						value={model.tag.formula ?? ''}
						onChange={(e) =>
							setModel({
								...model,
								tag: {
									...model.tag,
									formula: e.target.value,
								},
							})
						}
					/>
				</FormRow>
				<div>
					{/*<FormRow title='Входные параметры формулы'>
						{Object.entries(tag.calcInfo.inputs).map((x, index) => (
							<div
								key={index}
								style={{
									marginBottom: '.25em',
									display: 'grid',
									gridTemplateColumns: '3fr 2fr 1fr',
								}}
							>
								<Input
									value={x[0]}
									onChange={(e) =>
										setTag({
											...tag,
											Inputs: tag.Inputs.map((y, ind) =>
												ind !== index
													? y
													: {
															...y,
															VariableName:
																e.target.value,
													  },
											),
										})
									}
								/>
								<Select
									style={{ minWidth: '16em' }}
									options={inputs}
									value={x.InputTagId}
									onChange={(v) =>
										setTag({
											...tag,
											Inputs: tag.Inputs.map((y, ind) =>
												ind !== index
													? y
													: {
															...y,
															InputTagId: v,
													  },
											),
										})
									}
								></Select>
								<Button
									icon={<DeleteOutlined />}
									onClick={() => removeParam(index)}
								></Button>
							</div>
						))}
					</FormRow>*/}
					<Button
						icon={<AppstoreAddOutlined />}
						onClick={addParam}
					></Button>
				</div>
			</div>
			<div
				style={{
					display:
						model.sourceSwitcher === CustomSource.NotSet
							? 'block'
							: 'none',
				}}
			>
				<FormRow title='Используемый источник'>
					<Select
						options={[
							{
								value: CustomSource.NotSet,
								label: '? не выбран',
							},
							...sources,
						]}
						value={model.tag.sourceId}
						onChange={(value) =>
							setModel({
								...model,
								tag: {
									...model.tag,
									sourceId: value,
								},
							})
						}
						style={{ width: '100%' }}
					></Select>
				</FormRow>
				<div
					style={{
						display:
							model.tag.sourceId === CustomSource.NotSet
								? 'none'
								: 'inherit',
					}}
				>
					<FormRow title='Путь к данным в источнике'>
						<AutoComplete
							value={model.tag.sourceItem}
							options={items}
							onChange={(value) =>
								setModel({
									...model,
									tag: {
										...model.tag,
										sourceItem: value,
									},
								})
							}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
				<FormRow title='Значение'>
					<Space>
						<TagValueEl
							value={currentValue}
							type={model.tag.type}
						/>
					</Space>
				</FormRow>
			</div>
		</>
	)
}

export default TagForm
