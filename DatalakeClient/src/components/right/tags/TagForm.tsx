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
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import { CustomSources } from '../../../etc/customSources'
import { useFetching } from '../../../hooks/useFetching'
import router from '../../../router/router'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

export default function TagForm() {
	const { id } = useParams()
	const [tag, setTag] = useState({
		sourceInfo: {},
		mathInfo: {},
		calcInfo: {},
	} as TagInfo)
	const [name, setName] = useState('')
	const [sources, setSources] = useState(
		[] as { value: number; label: string }[],
	)
	const [items, setItems] = useState([] as { value: string }[])

	const [getItems] = useFetching(async () => {
		if (tag.sourceInfo.id === 0) return
		let res = await api.sourcesGetItemsWithTags(tag.sourceInfo.id)
		setItems(
			res.data.map((x) => ({
				value: x.itemInfo?.path ?? '',
				label: x.itemInfo?.path,
			})),
		)
	})

	const back = () => {
		router.navigate('/tags')
	}

	const [update] = useFetching(async () => {
		let res = await api.tagsUpdate(Number(id), tag)
		if (res.status === 200) back()
	})

	const [del] = useFetching(async () => {
		let res = await api.tagsDelete(Number(id))
		if (res.status === 200) back()
	})

	const addParam = () => {
		if (!tag.calcInfo) return
		let i = 1
		let exist = false
		let varName = 'x1'
		do {
			varName = 'x' + i++
			exist =
				// eslint-disable-next-line no-loop-func
				Object.keys(tag.calcInfo.inputs).filter((x) => x === varName)
					.length > 0
		} while (exist)
		let newInputs = tag.calcInfo.inputs
		newInputs[varName] = 0
		setTag({ ...tag, calcInfo: { ...tag.calcInfo, inputs: newInputs } })
	}

	/* const removeParam = (param: string) => {
		let newInputs = tag.calcInfo.inputs
		delete newInputs[param]
		setTag({ ...tag, calcInfo: { ...tag.calcInfo, inputs: newInputs } })
	} */

	const [load, , error] = useFetching(async () => {
		api.tagsRead(Number(id)).then((res) => {
			setTag(res.data)
			setName(res.data.name)
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
		if (tag.sourceInfo.id > 0) getItems()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [tag.sourceInfo.id])

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
				Тег {name}
			</Header>
			<FormRow title='Имя'>
				<Input
					value={tag.name}
					onChange={(e) => setTag({ ...tag, name: e.target.value })}
				/>
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={tag.description ?? ''}
					rows={4}
					style={{ resize: 'none' }}
					onChange={(e) =>
						setTag({ ...tag, description: e.target.value })
					}
				/>
			</FormRow>
			<FormRow title='Тип'>
				<Radio.Group
					buttonStyle='solid'
					value={tag.type}
					onChange={(e) => setTag({ ...tag, type: e.target.value })}
				>
					<Radio.Button value={TagType.String}>Строка</Radio.Button>
					<Radio.Button value={TagType.Number}>Число</Radio.Button>
					<Radio.Button value={TagType.Boolean}>Дискрет</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display: tag.type === TagType.Number ? 'block' : 'none',
				}}
			>
				<FormRow>
					<Checkbox
						checked={tag.mathInfo?.isScaling ?? false}
						onChange={(e) =>
							setTag({
								...tag,
								mathInfo: {
									...tag.mathInfo,
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
						display: tag.mathInfo?.isScaling ? 'block' : 'none',
					}}
				>
					<FormRow
						title='Шкала реальных значений'
						style={{ display: 'flex' }}
					>
						<InputNumber
							addonBefore='Min'
							value={tag.mathInfo?.minEu}
							onChange={(v) =>
								setTag({
									...tag,
									mathInfo: {
										...tag.mathInfo,
										minEu: Number(v),
									},
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={tag.mathInfo?.maxEu}
							onChange={(v) =>
								setTag({
									...tag,
									mathInfo: {
										...tag.mathInfo,
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
							value={tag.mathInfo.minRaw}
							onChange={(v) =>
								setTag({
									...tag,
									mathInfo: {
										...tag.mathInfo,
										minRaw: Number(v),
									},
								})
							}
						/>
						<InputNumber
							addonBefore='Max'
							value={tag.mathInfo.maxRaw}
							onChange={(v) =>
								setTag({
									...tag,
									mathInfo: {
										...tag.mathInfo,
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
					value={tag.sourceInfo.id}
					onChange={(e) =>
						setTag({
							...tag,
							sourceInfo: {
								...tag.sourceInfo,
								id: e.target.value,
							},
						})
					}
				>
					<Radio.Button value={CustomSources.Manual}>
						Мануальный
					</Radio.Button>
					<Radio.Button value={CustomSources.Calculated}>
						Вычисляемый
					</Radio.Button>
					<Radio.Button value={0}>Из источника</Radio.Button>
				</Radio.Group>
			</FormRow>
			<div
				style={{
					display:
						tag.sourceInfo.id !== CustomSources.Manual
							? 'block'
							: 'none',
				}}
			>
				<FormRow title='Интервал обновления в секундах (0, если только по изменению)'>
					<InputNumber
						value={tag.intervalInSeconds}
						onChange={(value) =>
							setTag({ ...tag, intervalInSeconds: Number(value) })
						}
					/>
				</FormRow>
			</div>
			<div
				style={{
					display:
						tag.sourceInfo.id === CustomSources.Calculated
							? 'block'
							: 'none',
				}}
			>
				<FormRow title='Формула для вычисления'>
					<Input
						value={tag.calcInfo.formula}
						onChange={(e) =>
							setTag({
								...tag,
								calcInfo: {
									...tag.calcInfo,
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
			<div style={{ display: tag.sourceInfo.id >= 0 ? 'block' : 'none' }}>
				<FormRow title='Используемый источник'>
					<Select
						options={sources}
						value={tag.sourceInfo.id}
						onChange={(value) =>
							setTag({
								...tag,
								sourceInfo: { ...tag.sourceInfo, id: value },
							})
						}
						style={{ width: '100%' }}
					></Select>
				</FormRow>
				<FormRow title='Путь к данным в источнике'>
					<AutoComplete
						value={tag.sourceInfo.item}
						options={items}
						onChange={(value) =>
							setTag({
								...tag,
								sourceInfo: { ...tag.sourceInfo, item: value },
							})
						}
						style={{ width: '100%' }}
					/>
				</FormRow>
			</div>
		</>
	)
}
