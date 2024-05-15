import { Button, Input, Popconfirm, Radio } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../../api/api'
import {
	AccessType,
	UserDetailInfo,
	UserUpdateRequest,
} from '../../../api/swagger/data-contracts'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

export default function UserForm() {
	const navigate = useNavigate()
	const { id } = useParams()
	const [user, setUser] = useState({} as UserDetailInfo)
	const [request, setRequest] = useState({} as UserUpdateRequest)
	const [isStatic, setStatic] = useState(false)
	const [wasStatic, setWasStatic] = useState(false)

	useEffect(load, [id])

	function load() {
		if (!!id) return
		api.usersReadWithDetails(String(id)).then((res) => {
			setStatic(!!res.data.staticHost)
			setWasStatic(!!res.data.staticHost)
			setUser(res.data)
			setRequest({
				loginName: res.data.loginName,
				accessType: res.data.accessType,
				fullName: res.data.fullName,
				createNewStaticHash: false,
				password: '',
				staticHost: res.data.staticHost,
			})
		})
	}

	function update() {
		api.usersUpdate(String(id), request).then(
			() => id !== user.loginName && navigate('/users/' + user.loginName),
		)
	}

	function del() {
		api.usersDelete(String(id)).then(() => navigate('/users/'))
	}

	function generateNewHash() {
		api.usersUpdate(String(id), {
			...request,
			createNewStaticHash: true,
		}).then(() => load())
	}

	return (
		<>
			<Header
				left={
					<Button onClick={() => navigate('/users/')}>
						Вернуться
					</Button>
				}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить эту учётную запись?'
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
				Учётная запись: {id}
			</Header>
			<form>
				<FormRow title='Имя учётной записи'>
					<Input
						value={user.loginName}
						onChange={(e) =>
							setUser({ ...user, loginName: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Имя пользователя'>
					<Input
						value={user.fullName ?? ''}
						onChange={(e) =>
							setUser({ ...user, fullName: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Тип доступа'>
					<Radio.Group
						buttonStyle='solid'
						value={isStatic}
						onChange={(e) => setStatic(e.target.value)}
					>
						<Radio.Button value={false}>Базовый</Radio.Button>
						<Radio.Button value={true}>Статичный</Radio.Button>
					</Radio.Group>
				</FormRow>

				{isStatic ? (
					<>
						<FormRow title='Адрес, с которого разрешен доступ'>
							<Input
								value={user.staticHost || ''}
								onChange={(e) =>
									setRequest({
										...request,
										staticHost: e.target.value,
									})
								}
							/>
						</FormRow>
						{wasStatic ? (
							<FormRow title='Ключ для доступа'>
								<Input disabled value={user.hash} />
								<div style={{ marginTop: '.5em' }}>
									<Button
										type='primary'
										onClick={() => {
											navigator.clipboard.writeText(
												user.hash ?? '',
											)
										}}
									>
										Скопировать
									</Button>
									&ensp;
									<Button onClick={generateNewHash}>
										Создать новый
									</Button>
								</div>
							</FormRow>
						) : (
							<></>
						)}
					</>
				) : (
					<FormRow title='Пароль'>
						<Input.Password
							value={request.password || ''}
							autoComplete='password'
							placeholder={
								wasStatic
									? 'Введите пароль'
									: 'Запишите новый пароль, если хотите его изменить'
							}
							onChange={(e) =>
								setRequest({
									...request,
									password: e.target.value,
								})
							}
						/>
					</FormRow>
				)}

				<FormRow title='Тип учётной записи'>
					<Radio.Group
						buttonStyle='solid'
						value={user.accessType}
						onChange={(e) =>
							setRequest({
								...request,
								accessType: e.target.value,
							})
						}
					>
						<Radio.Button value={AccessType.NOT}>
							Отключена
						</Radio.Button>
						<Radio.Button value={AccessType.USER}>
							Пользователь
						</Radio.Button>
						<Radio.Button value={AccessType.ADMIN}>
							Администратор
						</Radio.Button>
					</Radio.Group>
				</FormRow>
			</form>
		</>
	)
}
