import { Button, Input, Radio, Select } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../../../api/swagger-api'
import {
	AccessType,
	EnergoIdInfo,
	UserCreateRequest,
	UserType,
} from '../../../../api/swagger/data-contracts'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

export default function UserCreate() {
	const navigate = useNavigate()
	const [request, setRequest] = useState({
		accessType: AccessType.NotSet,
		password: '',
		staticHost: '',
		type: UserType.Local,
	} as UserCreateRequest)
	const [keycloakInfo, setKeycloakInfo] = useState({
		connected: false,
		energoIdUsers: [],
	} as EnergoIdInfo)

	function load() {
		api.usersGetEnergoIdList().then(
			(res) => !!res && setKeycloakInfo(res.data),
		)
	}

	function create() {
		api.usersCreate(request).then((res) => {
			navigate(routes.users.toUserForm(res.data))
		})
	}

	const onSearch = (value: string) => {
		console.log('search:', value)
	}

	// Filter `option.label` match the user type `input`
	const filterOption = (
		input: string,
		option?: { label: string; value: string },
	) => (option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	useEffect(load, [])

	return (
		<>
			<PageHeader
				left={
					<Button onClick={() => navigate(routes.users.list)}>
						Вернуться
					</Button>
				}
				right={
					<Button type='primary' onClick={create}>
						Создать
					</Button>
				}
			>
				Новая учётная запись
			</PageHeader>
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
						value={request.type}
						onChange={(e) =>
							setRequest({ ...request, type: e.target.value })
						}
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
							request.type === UserType.Local
								? 'inherit'
								: 'none',
					}}
				>
					<FormRow title='Логин для входа'>
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
				</div>

				<div
					style={{
						display:
							request.type === UserType.Local ||
							request.type === UserType.Static
								? 'inherit'
								: 'none',
					}}
				>
					<FormRow title='Полное имя'>
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
							request.type === UserType.Static
								? 'inherit'
								: 'none',
					}}
				>
					<FormRow title='Адрес, с которого разрешен доступ'>
						<Input
							value={request.staticHost || ''}
							placeholder='Если адрес не указан, доступ разрешен из любого источника'
							onChange={(e) =>
								setRequest({
									...request,
									staticHost: e.target.value,
								})
							}
						/>
					</FormRow>
				</div>

				<div
					style={{
						display:
							request.type === UserType.Local
								? 'inherit'
								: 'none',
					}}
				>
					<FormRow title='Пароль'>
						<Input.Password
							value={request.password || ''}
							autoComplete='password'
							placeholder='Введите пароль'
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
							request.type === UserType.EnergoId
								? 'inherit'
								: 'none',
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
							options={keycloakInfo.energoIdUsers.map((x) => ({
								value: x.energoIdGuid,
								label: x.fullName + ' (' + x.login + ')',
							}))}
							style={{ width: '100%' }}
							onChange={(value) => {
								const user = keycloakInfo.energoIdUsers.filter(
									(x) => x.energoIdGuid === value,
								)[0]
								if (user) {
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
