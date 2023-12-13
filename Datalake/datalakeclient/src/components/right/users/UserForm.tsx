import { useEffect, useState } from "react"
import { User } from "../../../@types/User"
import { useNavigate, useParams } from "react-router-dom"
import axios from "axios"
import { API } from "../../../router/api"
import Header from "../../small/Header"
import FormRow from "../../small/FormRow"
import { Button, Input, Popconfirm, Radio } from "antd"
import { AccessType } from "../../../@types/enums/AccessType"

export default function UserForm() {

	const navigate = useNavigate()
	const { id } = useParams()
	const [ user, setUser ] = useState({} as User)

	useEffect(load, [id])

	function load() {
		axios.post(API.auth.user, { name: id })
			.then(res => res.status === 200 && setUser(res.data))
	}

	function update() {
		axios.post(API.auth.update, { name: id })
			.then(res => res.status === 200 && id !== user.Name && navigate('/users/' + user.Name))
	}

	function del() {
		axios.post(API.auth.delete, { name: id })
			.then(res => res.status === 200 && navigate('/users/'))
	}

	return <>
		<Header
			left={<Button onClick={() => navigate('/users/')}>Вернуться</Button>}
			right={
				<>
					<Popconfirm
						title="Вы уверены, что хотите удалить эту учётную запись?"
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
		>
			Учётная запись: {id}
		</Header>
		<FormRow title="Имя учётной записи">
			<Input value={user.Name} onChange={e => setUser({ ...user, Name: e.target.value })} />
		</FormRow>
		<FormRow title="Имя пользователя">
			<Input value={user.FullName} onChange={e => setUser({ ...user, FullName : e.target.value })} />
		</FormRow>
		<FormRow title="Тип учётной записи">
			<Radio.Group buttonStyle="solid" value={user.AccessType} onChange={e => setUser({...user, AccessType: e.target.value })}>
				<Radio.Button value={AccessType.NOT}>Отключена</Radio.Button>
				<Radio.Button value={AccessType.USER}>Пользователь</Radio.Button>
				<Radio.Button value={AccessType.ADMIN}>Администратор</Radio.Button>
			</Radio.Group>
		</FormRow>
	</>
}