import { Button, Input, Popconfirm, Radio, Select } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	AccessType,
	UserEnergoIdInfo,
	UserType,
	UserUpdateRequest,
} from '../../../api/swagger/data-contracts'
import routes from '../../../router/routes'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'

export default function UserForm() {
	const navigate = useNavigate()
	const { id } = useParams()
	const [user, setUser] = useState({ oldType: UserType.Local, hash: '' })
	const [request, setRequest] = useState({} as UserUpdateRequest)
	const [newType, setNewType] = useState(UserType.Local)
	const [keycloakUsers, setKeycloakUsers] = useState([] as UserEnergoIdInfo[])

	useEffect(load, [id])

	function load() {
		if (!id) return
		api.usersReadWithDetails(String(id)).then((res) => {
			setNewType(res.data.type)
			setUser({ oldType: res.data.type, hash: res.data.hash ?? '' })
			setRequest({
				login: res.data.login,
				accessType: res.data.accessType,
				fullName: res.data.fullName,
				createNewStaticHash: false,
				password: '',
				staticHost: res.data.staticHost,
				energoIdGuid: res.data.energoIdGuid,
				type: res.data.type,
			})
		})
		api.usersGetEnergoIdList({ currentUserGuid: id }).then((res) =>
			setKeycloakUsers(res.data),
		)
	}

	function update() {
		api.usersUpdate(String(id), request).then((res) => {
			if (res.status >= 300) return
			load()
		})
	}

	function del() {
		api.usersDelete(String(id)).then(() => navigate(routes.Users.List))
	}

	function generateNewHash() {
		api.usersUpdate(String(id), {
			...request,
			createNewStaticHash: true,
		}).then(() => load())
	}

	const onSearch = (value: string) => {
		console.log('search:', value)
	}

	// Filter `option.label` match the user type `input`
	const filterOption = (
		input: string,
		option?: { label: string; value: string },
	) => (option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	return (
		<>
			<Header
				left={
					<Button onClick={() => navigate(routes.Users.List)}>
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
				<FormRow title='Уровень глобального доступа'>
					<Radio.Group
						buttonStyle='solid'
						value={request.accessType}
						onChange={(e) =>
							setRequest({
								...request,
								accessType: e.target.value,
							})
						}
					>
						<Radio.Button value={AccessType.NotSet}>
							Отключена
						</Radio.Button>
						<Radio.Button value={AccessType.NoAccess}>
							Заблокирована
						</Radio.Button>
						<Radio.Button value={AccessType.Viewer}>
							Наблюдатель
						</Radio.Button>
						<Radio.Button value={AccessType.User}>
							Пользователь
						</Radio.Button>
						<Radio.Button value={AccessType.Admin}>
							Администратор
						</Radio.Button>
					</Radio.Group>
				</FormRow>

				<FormRow title='Тип учетной записи'>
					<Radio.Group
						buttonStyle='solid'
						value={newType}
						onChange={(e) => setNewType(e.target.value)}
					>
						<Radio.Button value={UserType.Static}>
							Статичная учетная запись
						</Radio.Button>
						<Radio.Button value={UserType.Local}>
							Базовая учетная запись
						</Radio.Button>
						<Radio.Button value={UserType.EnergoId}>
							Учетная запись EnergoID
						</Radio.Button>
					</Radio.Group>
				</FormRow>

				<div
					style={{
						display:
							newType === UserType.Local ||
							newType === UserType.Static
								? 'inherit'
								: 'none',
					}}
				>
					<FormRow title='Имя учетной записи'>
						<Input
							value={request.fullName ?? ''}
							onChange={(e) =>
								setRequest({
									...request,
									fullName: e.target.value,
								})
							}
						/>
					</FormRow>
				</div>

				<div
					style={{
						display:
							newType === UserType.Static ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Адрес, с которого разрешен доступ'>
						<Input
							value={request.staticHost || ''}
							onChange={(e) =>
								setRequest({
									...request,
									staticHost: e.target.value,
								})
							}
						/>
					</FormRow>
					{user.oldType === UserType.Static ? (
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
				</div>

				<div
					style={{
						display:
							newType === UserType.Local ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Имя для входа'>
						<Input
							value={request.login ?? ''}
							onChange={(e) =>
								setRequest({
									...request,
									login: e.target.value,
								})
							}
						/>
					</FormRow>
					<FormRow title='Пароль'>
						<Input.Password
							value={request.password || ''}
							autoComplete='password'
							placeholder={
								user.oldType !== UserType.Local
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
				</div>

				<div
					style={{
						display:
							newType === UserType.EnergoId ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Учетная запись на сервере EnergoID'>
						<Select
							showSearch
							optionFilterProp='children'
							onSearch={onSearch}
							filterOption={filterOption}
							value={request.energoIdGuid || ''}
							placeholder='Укажите учетную запись EnergoID'
							options={keycloakUsers.map((x) => ({
								value: x.energoIdGuid,
								label: x.fullName + ' (' + x.login + ')',
							}))}
							style={{ width: '100%' }}
							onChange={(value) => {
								let user = keycloakUsers.filter(
									(x) => x.energoIdGuid === value,
								)[0]
								if (!!user) {
									setRequest({
										...request,
										energoIdGuid: value,
										login: user.login,
										fullName: user.fullName,
									})
								}
							}}
						/>
					</FormRow>
				</div>
			</form>
		</>
	)
}
