import { Input, Select, InputNumber, Popconfirm, Button, AutoComplete } from "antd";
import { useEffect, useState } from "react";
import { useFetching } from "../../../hooks/useFetching";
import { Tag } from "../../../@types/tag";
import axios from "axios";
import { SourceType } from "../../../@types/source";
import { Navigate, useParams } from "react-router-dom";
import Header from "../../small/Header";
import router from "../../../router/router";
import FormRow from "../../small/FormRow";

export default function TagForm() {

	const { id } = useParams()
	const [ tag, setTag ] = useState({} as Tag)
	const [ name, setName ] = useState('')
	const [ sources, setSources ] = useState([] as { value: number, label: string }[])
	const [ items, setItems ] = useState([] as { value: string }[])

	const [ getItems ] = useFetching(async () => {
		if (tag.SourceId === 0) return
		let res = await axios.post('sources/items', { Id: tag.SourceId })
		setItems(res.data.map((x: string) => ({ value: x })))
	})

	const back = () => { router.navigate('/tags') }

	const [ update ] = useFetching(async () => {
		let res = await axios.post('tags/update', tag)
		if (res.data.Done) back()
	})

	const [ del ] = useFetching(async () => {
		let res = await axios.post('tags/delete', tag)
		if (res.data.Done) back()
	})

	const [ load, , error ] = useFetching(async () => {
		let res = await axios.post('tags/read', { id })
		setTag(res.data)
		setName(res.data.Name)

		res = await axios.post('sources/list')
		setSources(res.data.map((x: SourceType) => ({ value: x.Id, label: x.Name })))
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
				<FormRow title="Тип">
					<Select
						options={[{ value: 0, label: 'строка' }, { value: 1, label: 'число' }, { value: 2, label: 'дискрет' }]}
						value={tag.Type}
						onChange={value => setTag({ ...tag, Type: value })}
						style={{ width: '100%' }}
					></Select>
				</FormRow>
				<FormRow title="Описание">
					<Input.TextArea
						value={tag.Description}
						rows={4}
						style={{ resize: 'none' }}
						onChange={e => setTag({ ...tag, Description: e.target.value })}
					/>
				</FormRow>
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
				<FormRow title="Интервал опроса в секундах (0, если только по изменению)">
					<InputNumber value={tag.Interval} onChange={value => setTag({ ...tag, Interval: Number(value) })} />
				</FormRow>
			</>
	)
}