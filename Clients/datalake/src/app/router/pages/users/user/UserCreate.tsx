import FormRow from '@/app/components/FormRow'
import UserIcon from '@/app/components/icons/UserIcon'
import NoAccessEl from '@/app/components/NoAccessEl'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import { AccessType, UserCreateRequest, UserEnergoIdInfo, UserType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { accessOptions } from '@/types/accessOptions'
import { Button, Input, Radio, Select } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const UserCreate = observer(() => {
	useDatalakeTitle('Пользователи', 'Добавление')
	const store = useAppStore()
	const navigate = useNavigate()
	const [request, setRequest] = useState({
		accessType: AccessType.None,
		password: '',
		staticHost: '',
		type: UserType.Local,
	} as UserCreateRequest)
	const [keycloakInfo, setKeycloakInfo] = useState<UserEnergoIdInfo[]>([])

	function load() {
		store.api
			.inventoryEnergoIdGetEnergoId()
			.then((res) => !!res && setKeycloakInfo(res.data.sort((a, b) => a.fullName.localeCompare(b.fullName))))
	}

	async function create() {
		try {
			const userGuid = await store.usersStore.createUser(request)
			if (userGuid) {
				navigate(routes.users.toUserForm(userGuid))
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create user'), {
				component: 'UserCreate',
				action: 'create',
			})
		}
	}

	// Filter `option.label` match the user type `input`
	const filterOption = (input: string, option?: { label: string; value: string }) =>
		(option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	useEffect(load, [store.api])

	if (!store.hasGlobalAccess(AccessType.Admin)) return <NoAccessEl />

	return (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.users.list)}>Вернуться</Button>]}
				right={[
					<Button type='primary' onClick={create}>
						Создать
					</Button>,
				]}
				icon={<UserIcon />}
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
						display: request.type === UserType.Local ? 'inherit' : 'none',
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
