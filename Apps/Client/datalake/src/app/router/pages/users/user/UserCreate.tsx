import api from '@/api/swagger-api'
import NoAccessPage from '@/app/components/NoAccessPage'
import { user } from '@/state/user'
import { accessOptions } from '@/types/accessOptions'
import { Button, Input, Radio, Select } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AccessType, UserCreateRequest, UserEnergoIdInfo, UserType } from '../../../../generated/data-contracts'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

const UserCreate = observer(() => {
	const navigate = useNavigate()
	const [request, setRequest] = useState({
		accessType: AccessType.NotSet,
		password: '',
		staticHost: '',
		type: UserType.Local,
	} as UserCreateRequest)
	const [keycloakInfo, setKeycloakInfo] = useState<UserEnergoIdInfo[]>([])

	function load() {
		api
			.usersGetEnergoId()
			.then((res) => !!res && setKeycloakInfo(res.data.sort((a, b) => a.fullName.localeCompare(b.fullName))))
	}

	function create() {
		api.usersCreate(request).then((res) => {
			navigate(routes.users.toUserForm(res.data.guid))
		})
	}

	// Filter `option.label` match the user type `input`
	const filterOption = (input: string, option?: { label: string; value: string }) =>
		(option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	useEffect(load, [])

	if (!user.hasGlobalAccess(AccessType.Admin)) return <NoAccessPage />

	return (
		<>
			<PageHeader
				left={<Button onClick={() => navigate(routes.users.list)}>Вернуться</Button>}
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
					<Select
						value={request.accessType}
						defaultValue={request.accessType}
						options={accessOptions}
						onChange={(e) =>
							setRequest({
								...request,
								accessType: e,
							})
						}
					></Select>
				</FormRow>

				<FormRow title='Тип учетной записи'>
					<Radio.Group
						buttonStyle='solid'
						value={request.type}
						onChange={(e) => setRequest({ ...request, type: e.target.value })}
					>
						<Radio.Button value={UserType.Static}>Статичная учетная запись</Radio.Button>
						<Radio.Button value={UserType.Local}>Базовая учетная запись</Radio.Button>
						<Radio.Button value={UserType.EnergoId}>Учетная запись EnergoID</Radio.Button>
					</Radio.Group>
				</FormRow>

				<div
					style={{
						display: request.type === UserType.Local ? 'inherit' : 'none',
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
						display: request.type === UserType.Local || request.type === UserType.Static ? 'inherit' : 'none',
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
						display: request.type === UserType.Static ? 'inherit' : 'none',
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
						display: request.type === UserType.Local ? 'inherit' : 'none',
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
						display: request.type === UserType.EnergoId ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Учетная запись на сервере EnergoID'>
						<Select
							showSearch
							optionFilterProp='children'
							filterOption={filterOption}
							value={request.energoIdGuid || ''}
							placeholder='Укажите учетную запись EnergoID'
							options={keycloakInfo.map((x) => ({
								value: x.energoIdGuid,
								disabled: !!x.userGuid,
								label: (x.userGuid ? '(уже добавлен) ' : '') + x.fullName + ' (' + x.email + ')',
							}))}
							style={{ width: '100%' }}
							onChange={(value) => {
								const user = keycloakInfo.filter((x) => x.energoIdGuid === value)[0]
								if (user) {
									setRequest({
										...request,
										energoIdGuid: value,
										login: user.email,
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
})

export default UserCreate
