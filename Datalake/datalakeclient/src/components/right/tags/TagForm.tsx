import { Input, Select, InputNumber, Popconfirm, Button, AutoComplete, Radio, Checkbox } from "antd";
import { useEffect, useState } from "react";
import { useFetching } from "../../../hooks/useFetching";
import { Tag } from "../../../@types/Tag";
import axios from "axios";
import { TagSource } from "../../../@types/Source";
import { Navigate, useParams } from "react-router-dom";
import Header from "../../small/Header";
import router from "../../../router/router";
import FormRow from "../../small/FormRow";
import { AppstoreAddOutlined, DeleteOutlined } from "@ant-design/icons";

export default function TagForm() {

	const { id } = useParams()
	const [ tag, setTag ] = useState({} as Tag)
	const [ name, setName ] = useState('')
	const [ sources, setSources ] = useState([] as { value: number, label: string }[])
	const [ items, setItems ] = useState([] as { value: string }[])
	const [ inputs, setInputs ] = useState([] as { value: number, label: string }[])

	const [ getItems ] = useFetching(async () => {
		if (tag.SourceId === 0) return
		let res = await axios.post('sources/items', { Id: tag.SourceId })
		setItems(res.data.map((x: string) => ({ value: x })))
	})

	const back = () => { router.navigate('/tags') }

	const [ update ] = useFetching(async () => {
		let res = await axios.post('tags/update', { tag: tag })
		if (res.data.Done) back()
	})

	const [ del ] = useFetching(async () => {
		let res = await axios.post('tags/delete', tag)
		if (res.data.Done) back()
	})

	const addParam = () => {
		let i = 1
		let exist = false
		let varName = 'x1'
		do {
			varName = 'x' + i++
			// eslint-disable-next-line
			exist = tag.Inputs.filter(x => x.VariableName === varName).length > 0
		}
		while (exist)
		let newInputs = tag.Inputs
		newInputs.push({ TagId: tag.Id, VariableName: varName, InputTagId: 0})
		setTag({ ...tag, Inputs: newInputs })
	}

	const removeParam = (i: number) => {
		let newInputs = tag.Inputs
		newInputs.splice(i, 1)
		setTag({ ...tag, Inputs: newInputs })
	}

	const [ load, , error ] = useFetching(async () => {
		let res = await axios.post('tags/read', { id })
		setTag(res.data)
		setName(res.data.Name)

		res = await axios.post('tags/inputs', { id })
		setInputs([...res.data.map((x: Tag) => ({ value: x.Id, label: x.Name })), { value: 0, label: '?'} ])

		res = await axios.post('sources/list')
		setSources([ ...res.data.map((x: TagSource) => ({ value: x.Id, label: x.Name })), { value: 0, label: '?'} ])
	})
	
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { if (!!id) load() }, [id])
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { if (tag.SourceId > 0) getItems() }, [tag.SourceId])

	return (
		error
			? <Navigate to="/offline" />
			: <>
				<Header
					left={<Button onClick={() => router.navigate('/tags')}>Вернуться</Button>}
					right={
						<>
							<Popconfirm
								title="Вы уверены, что хотите удалить этот тег?"
								placement="bottom"
								onConfirm={del}
								okText="Да"
								cancelText="Нет"
							>
								<Button>Удалить</Button>
							</Popconfirm>
							<Button type="primary" onClick={update}>Сохранить</Button>
						</>
					}
				>Тег {name}</Header>
				<FormRow title="Имя">
					<Input value={tag.Name} onChange={e => setTag({ ...tag, Name: e.target.value })} />
				</FormRow>
				<FormRow title="Описание">
					<Input.TextArea
						value={tag.Description}
						rows={4}
						style={{ resize: 'none' }}
						onChange={e => setTag({ ...tag, Description: e.target.value })}
					/>
				</FormRow>
				<FormRow title="Интервал обновления в секундах (0, если только по изменению)">
					<InputNumber value={tag.Interval} onChange={value => setTag({ ...tag, Interval: Number(value) })} />
				</FormRow>
				<FormRow title="Тип">
					<Radio.Group buttonStyle="solid" value={tag.Type} onChange={e => setTag({...tag, Type: e.target.value })}>
						<Radio.Button value={0}>Строка</Radio.Button>
						<Radio.Button value={1}>Число</Radio.Button>
						<Radio.Button value={2}>Дискрет</Radio.Button>
					</Radio.Group>
				</FormRow>
				<div style={{ display: tag.Type === 1 ? 'block' : 'none'}}>
					<FormRow>
						<Checkbox checked={tag.IsScaling} onChange={e => setTag({ ...tag, IsScaling: e.target.checked })}>Преобразование по шкалам</Checkbox>
					</FormRow>
					<div style={{ display: tag.IsScaling ? 'block' : 'none'}}>
						<FormRow title="Шкала реальных значений" style={{ display: 'flex' }}>
							<InputNumber addonBefore="Min" value={tag.MinEU} onChange={v => setTag({ ...tag, MinEU: Number(v) })} />
							<InputNumber addonBefore="Max" value={tag.MaxEU} onChange={v => setTag({ ...tag, MaxEU: Number(v) })} />
						</FormRow>
						<FormRow title="Шкала преобразованных значений" style={{ display: 'flex' }}>
							<InputNumber addonBefore="Min" value={tag.MinRaw} onChange={v => setTag({ ...tag, MinRaw: Number(v) })} />
							<InputNumber addonBefore="Max" value={tag.MaxRaw} onChange={v => setTag({ ...tag, MaxRaw: Number(v) })} />
						</FormRow>
					</div>
				</div>
				<FormRow title="Способ получения">
					<Radio.Group buttonStyle="solid" value={tag.IsCalculating} onChange={e => setTag({...tag, IsCalculating: e.target.value })}>
						<Radio.Button value={true}>Вычисляемый</Radio.Button>
						<Radio.Button value={false}>Из источника</Radio.Button>
					</Radio.Group>
				</FormRow>
				<div style={{ display: tag.IsCalculating ? 'block' : 'none' }}>
					<FormRow title="Формула для вычисления">
						<Input value={tag.Formula} onChange={e => setTag({...tag, Formula: e.target.value })} />
					</FormRow>
					<div>
						<FormRow title="Входные параметры формулы">
						{tag.Inputs && tag.Inputs.map((x, index) => (
							<div key={index} style={{ marginBottom: '.25em', display: 'grid', gridTemplateColumns: '3fr 2fr 1fr' }}>
								<Input
									value={x.VariableName}
									onChange={e => setTag({ 
										...tag,
										Inputs: tag.Inputs.map((y, ind) => ind !== index ? y : { ...y, VariableName: e.target.value } )
									})}
								/>
								<Select
									style={{ minWidth: '16em' }}
									options={inputs}
									value={x.InputTagId}
									onChange={v => setTag({
										...tag,
										Inputs: tag.Inputs.map((y, ind) => ind !== index ? y : { ...y, InputTagId: v } )
									})}
								></Select>
								<Button icon={<DeleteOutlined />} onClick={() => removeParam(index)}></Button>
							</div>
						))}
						</FormRow>
						<Button icon={<AppstoreAddOutlined />} onClick={addParam}></Button>
					</div>
				</div>
				<div style={{ display: !tag.IsCalculating ? 'block' : 'none'}}>
					<FormRow title="Используемый источник">
						<Select
							options={sources}
							value={tag.SourceId}
							onChange={value => setTag({ ...tag, SourceId: value })}
							style={{ width: '100%' }}
						></Select>
					</FormRow>
					<FormRow title="Путь к данным в источнике">
						<AutoComplete
							value={tag.SourceItem}
							options={items}
							onChange={value => setTag({ ...tag, SourceItem: value })}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
			</>
	)
}