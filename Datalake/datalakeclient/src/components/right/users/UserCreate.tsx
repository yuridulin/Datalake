import { useState } from "react"
import { User } from "../../../@types/User"
import { useNavigate } from "react-router-dom"
import axios from "axios"
import { API } from "../../../router/api"
import Header from "../../small/Header"
import FormRow from "../../small/FormRow"
import { Button, Input, Radio } from "antd"
import { AccessType } from "../../../@types/enums/AccessType"

export default function UserCreate() {

	const navigate = useNavigate()
	const [ user, setUser ] = useState({ AccessType: AccessType.NOT } as User)

	function create() {
		axios.post(API.auth.create, user)
			.then(res => res.status === 200 && navigate('/users/'))
	}

	return <>
		<Header
			left={<Button onClick={() => navigate('/users/')}>Вернуться</Button>}
			right={<Button type="primary" onClick={create}>Создать</Button>}
		>
			Новая учётная запись
		</Header>
		<form>
			<FormRow title="Имя учётной записи">
				<Input value={user.Name} onChange={e => setUser({ ...user, Name: e.target.value })} />
			</FormRow>
			<FormRow title="Имя пользователя">
				<Input value={user.FullName} onChange={e => setUser({ ...user, FullName: e.target.value })} />
			</FormRow>
			<FormRow title="Пароль">
				<Input.Password value={user.Password} autoComplete="password" onChange={e => setUser({ ...user, Password: e.target.value })} />
			</FormRow>
			<FormRow title="Тип учётной записи">
				<Radio.Group buttonStyle="solid" value={user.AccessType} onChange={e => setUser({...user, AccessType: e.target.value })}>
					<Radio.Button value={AccessType.NOT}>Отключена</Radio.Button>
					<Radio.Button value={AccessType.USER}>Пользователь</Radio.Button>
					<Radio.Button value={AccessType.ADMIN}>Администратор</Radio.Button>
				</Radio.Group>
			</FormRow>
		</form>
	</>
}