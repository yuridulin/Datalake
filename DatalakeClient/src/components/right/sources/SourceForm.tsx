import { Button, Input, Popconfirm, Radio } from "antd"
import { useState, useEffect } from "react"
import { TagSource } from "../../../@types/Source"
import { useFetching } from "../../../hooks/useFetching"
import { Navigate, useParams } from "react-router-dom"
import router from "../../../router/router"
import axios from "axios"
import Header from "../../small/Header"
import FormRow from "../../small/FormRow"
import SourceItems from "./SourceItems"
import { SourceType } from "../../../@types/enums/SourceType"
import { API } from "../../../router/api"

export default function SourceForm() {

	const { id } = useParams();

	const [ source, setSource ] = useState({} as TagSource)
	const [ name, setName ] = useState('')

	const [ read, , error ] = useFetching(async () => {
		let res = await axios.post(API.sources.readById, { id })
		setSource(res.data)
		setName(res.data.Name)
	})

	const [ update ] = useFetching(async () => {
		axios.post(API.sources.update, source).then(() => router.navigate('/sources'))
	})

	const [ del ] = useFetching(async () => {
		axios.post(API.sources.del, source).then(() => router.navigate('/sources'))
	})

	const [ createTag ] = useFetching(async () => {
		let res = await axios.post(API.tags.create, { sourceId: source.Id })
		if (res.data.Done) read()
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { read() }, [id])

	console.log()

	return (
		error
		? <Navigate to='/offline' />
		: <>
			<Header
				left={<Button onClick={() => router.navigate('/sources')}>Вернуться</Button>}
				right={<><Popconfirm
					title='Удалить источник?'
					description='Теги, связанные с источником, будут сохранены, но не смогут получать обновления'
					onConfirm={del}
					okText='Удалить'
					cancelText='Отмена'><Button>Удалить</Button>
				</Popconfirm>
				<Button onClick={update}>Сохранить</Button></>}
			>
				Источник: {name}
			</Header>
			<FormRow title="Имя">
				<Input value={source.Name} onChange={e => setSource({ ...source, Name: e.target.value })} />
			</FormRow>
			<FormRow title="Тип источника">
				<Radio.Group buttonStyle="solid" value={source.Type} onChange={e => setSource({...source, Type: e.target.value })}>
					{Object.values(SourceType).filter(x => !(x as string).length).map(x => 
						<Radio.Button key={x} value={Number(x)}>{SourceType[Number(x)]}</Radio.Button>
					)}
				</Radio.Group>
			</FormRow>
			<FormRow title="Адрес">
				<Input value={source.Address} onChange={e => setSource({ ...source, Address: e.target.value })} />
			</FormRow>
			<Button onClick={createTag}>Добавить тег</Button>
			<br />
			<br />
			<SourceItems type={source.Type} id={source.Id} />
		</>
	)
}