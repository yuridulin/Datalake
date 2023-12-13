import { useEffect, useState } from "react"
import { User } from "../../../@types/User"
import axios from "axios"
import { API } from "../../../router/api"
import Header from "../../small/Header"
import { Button, Input } from "antd"
import FormRow from "../../small/FormRow"
import { AccessTypeDescription } from "../../../@types/enums/AccessType"
import { NavLink, useNavigate } from "react-router-dom"

export default function UsersList() {

	const navigate = useNavigate()
	const [ users, setUsers ] = useState([] as User[])
	const [ search, setSearch ] = useState('')

	useEffect(load, [])
	function load() {
		axios
			.post(API.auth.users)
			.then(res => setUsers(res.data))
	}

	function create() {
		navigate('/users/create')
	}

	return <>
		<Header
			right={<Button onClick={create}>Добавить пользователя</Button>}
		>Список пользователей</Header>
		{users.length > 0 && <>
			<FormRow title="Поиск">
				<Input value={search} onChange={e => setSearch(e.target.value)} placeholder="введите поисковый запрос..." />
			</FormRow>
			<div className="table">
				<div className="table-header">
					<span style={{ width: '12em' }}>Логин</span>
					<span style={{ width: '12em' }}>Уровень доступа</span>
					<span>Имя</span>
				</div>
				{users.filter(x => (x.Name + x.FullName + AccessTypeDescription(x.AccessType)).toLowerCase().trim().includes(search.toLowerCase())).map(x =>
					<div className="table-row" key={x.Name}>
						<span>
							<NavLink to={'/users/' + x.Name}>
								<Button>{x.Name}</Button>
							</NavLink>
						</span>
						<span>{AccessTypeDescription(x.AccessType)}</span>
						<span>{x.FullName}</span>
					</div>
				)}
			</div>
		</>}
	</>
}