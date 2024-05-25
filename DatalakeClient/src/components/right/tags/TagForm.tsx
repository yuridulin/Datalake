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
} from 'antd'
import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import api from '../../../api/api'
import { TagType, TagUpdateRequest } from '../../../api/swagger/data-contracts'
import { CustomSource } from '../../../etc/customSource'
import { useFetching } from '../../../hooks/useFetching'
import router from '../../../router/router'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

export default function TagForm() {
	//#region Данные

	const { id } = useParams()

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

	const [getItems] = useFetching(async () => {
		if (model.tag.sourceId <= 0) return
		let res = await api.sourcesGetItems(model.tag.sourceId)
		setItems(
			res.data.map((x) => ({
				value: x.path ?? '',
			})),
		)
	})

	const [load, , error] = useFetching(async () => {
		api.tagsRead(Number(id)).then((res) => {
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
	})

	useEffect(() => {
		if (!!id) load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	useEffect(() => {
		if (model.tag.sourceId > 0) getItems()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [model.tag.sourceId])

	//#endregion

	//#region Действия

	const back = () => {
		router.navigate('/tags')
	}

	const [update] = useFetching(async () => {
		let res = await api.tagsUpdate(Number(id), model.tag)
		if (res.status === 200) back()
	})

	const [del] = useFetching(async () => {
		let res = await api.tagsDelete(Number(id))
		if (res.status === 200) back()
	})

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

	return error ? (
		<Navigate to='/offline' />
	) : (
		<>
			<Header
				left={
					<Button onClick={() => router.navigate('/tags')}>
						Вернуться
					</Button>
				}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить этот тег?'
							placement='bottom'
							onConfirm={del}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						<Button type='primary' onClick={update}>
							Сохранить
						</Button>
					</>
				}
			>
				Тег {model.oldName}
			</Header>
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
			</div>
		</>
	)
}
