import { Button, Input, Popconfirm } from "antd"
import { useState, useEffect } from "react"
import { TagSource } from "../../../@types/Source"
import { useFetching } from "../../../hooks/useFetching"
import { Navigate, useParams } from "react-router-dom"
import router from "../../../router/router"
import axios from "axios"
import Header from "../../small/Header"
import FormRow from "../../small/FormRow"
import SourceItems from "./SourceItems"

export default function SourceForm() {

	const { id } = useParams();

	const [ source, setSource ] = useState({} as TagSource)
	const [ name, setName ] = useState('')

	const [ read, , error ] = useFetching(async () => {
		let res = await axios.post('sources/read/', { id })
		setSource(res.data)
		setName(res.data.Name)
	})

	const [ update ] = useFetching(async () => {
		axios.post('sources/update/' + id, source).then(() => router.navigate('/sources'))
	})

	const [ del ] = useFetching(async () => {
		axios.post('sources/delete/' + id, source).then(() => router.navigate('/sources'))
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { read() }, [id])

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
			<FormRow title="Адрес">
				<Input value={source.Address} onChange={e => setSource({ ...source, Address: e.target.value })} />
			</FormRow>
			<SourceItems id={source.Id} />
		</>
	)
}