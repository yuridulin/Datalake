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
	const [ isStatic, setStatic ] = useState(false)
	const [ wasStatic, setWasStatic ] = useState(false)

	useEffect(load, [id])

	function load() {
		axios.post(API.auth.user, { name: id })
			.then(res => {
				if (res.status === 200) {
					setStatic(!!res.data.StaticHost)
					setWasStatic(!!res.data.StaticHost)
					setUser(res.data)
				}
			})
	}

	function update() {
		axios.post(API.auth.update, { ...user, newName: user.Name })
			.then(res => res.status === 200 && id !== user.Name && navigate('/users/' + user.Name))
	}

	function del() {
		axios.post(API.auth.delete, { name: id })
			.then(res => res.status === 200 && navigate('/users/'))
	}

	function generateNewHash() {
		axios.post(API.auth.update, { name: user.Name, newHash: true })
			.then(res => res.status === 200 && load())
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
		<form>
			<FormRow title="Имя учётной записи">
				<Input value={user.Name} onChange={e => setUser({ ...user, Name: e.target.value })} />
			</FormRow>
			<FormRow title="Имя пользователя">
				<Input value={user.FullName} onChange={e => setUser({ ...user, FullName : e.target.value })} />
			</FormRow>
			<FormRow title="Тип доступа">
				<Radio.Group buttonStyle="solid" value={isStatic} onChange={e => setStatic(e.target.value)}>
					<Radio.Button value={false}>Базовый</Radio.Button>
					<Radio.Button value={true}>Статичный</Radio.Button>
				</Radio.Group>
			</FormRow>
			
			{isStatic
			? <>
				<FormRow title="Адрес, с которого разрешен доступ">
					<Input value={user.StaticHost || ''} onChange={e => setUser({ ...user, StaticHost: e.target.value })} />
				</FormRow>
				{wasStatic 
				? <FormRow title="Ключ для доступа">
					<Input disabled value={user.Hash} />
					<div style={{ marginTop: '.5em' }}>
						<Button type="primary" onClick={() => {navigator.clipboard.writeText(user.Hash)}}>Скопировать</Button>
						&ensp;
						<Button onClick={generateNewHash}>Создать новый</Button>
					</div>
				</FormRow>
				: <></>}
			</>
			: <FormRow title="Пароль">
				<Input.Password
					value={user.Password || ''}
					autoComplete="password"
					placeholder={wasStatic ? "Введите пароль" : "Запишите новый пароль, если хотите его изменить"}
					onChange={e => setUser({ ...user, Password: e.target.value})}
				/>
			</FormRow>}

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