import FormRow from '@/app/components/FormRow'
import UserIcon from '@/app/components/icons/UserIcon'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserEnergoIdInfo, UserInfo, UserType, UserUpdateRequest } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { accessOptions } from '@/types/accessOptions'
import { Button, Input, Popconfirm, Radio, Select, Spin, Tag } from 'antd'
import { DefaultOptionType } from 'antd/es/select'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

type UserInfoProps = UserInfo & {
	oldType: UserType
}

interface EnergoIdOption extends DefaultOptionType {
	value: string // energoIdGuid
	label: React.ReactNode // то, что видим в списке и в закрытом состоянии
	search: string // чисто для поиска
	disabled?: boolean
	data: UserEnergoIdInfo // твои исходные данные
}

const UserForm = observer(() => {
	const store = useAppStore()
	const navigate = useNavigate()
	const { id } = useParams()
	useDatalakeTitle('Пользователи', id, 'Изменение')
	const [oldName, setOldName] = useState('')
	const [userInfo, setUser] = useState({
		oldType: UserType.Local,
	} as UserInfoProps)
	const [request, setRequest] = useState({} as UserUpdateRequest)
	const [newType, setNewType] = useState(UserType.Local)
	const [keycloakUsers, setKeycloakUsers] = useState<UserEnergoIdInfo[]>([])
	// Получаем пользователя из store (реактивно через MobX)
	const userData = id ? store.usersStore.getUserByGuid(id) : undefined
	const isLoading = id ? store.usersStore.isLoadingUsers() : false

	// Обновляем локальное состояние при загрузке из store
	useEffect(() => {
		if (!userData) return

		setNewType(userData.type)
		setUser({
			...userData,
			oldType: userData.type,
		})
		setRequest({
			login: userData.login,
			accessType: userData.accessType,
			fullName: userData.fullName,
			createNewStaticHash: false,
			password: '',
			energoIdGuid: userData.type === UserType.EnergoId ? userData.guid : null,
			type: userData.type,
		})
		setOldName(userData.fullName ?? id)
	}, [userData, id])

	// Загружаем список EnergoId пользователей
	useEffect(() => {
		store.api.inventoryEnergoIdGetEnergoId().then((res) => !!res && setKeycloakUsers(res.data))
	}, [store.api])

	const update = async () => {
		try {
			const response = await store.api.inventoryUsersUpdate(String(id), request)
			if (response.status >= 300) return
			// Инвалидируем кэш и обновляем данные
			if (id) {
				store.usersStore.invalidateUser(id)
				store.usersStore.refreshUsers()
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update user'), {
				component: 'UserForm',
				action: 'update',
				userId: id,
			})
		}
	}

	const del = async () => {
		try {
			await store.api.inventoryUsersDelete(String(id))
			// Инвалидируем кэш
			if (id) {
				store.usersStore.invalidateUser(id)
			}
			navigate(routes.users.list)
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete user'), {
				component: 'UserForm',
				action: 'del',
				userId: id,
			})
		}
	}

	const options: EnergoIdOption[] = keycloakUsers.map((x) => ({
		value: x.energoIdGuid,
		disabled: Boolean(x.userGuid && x.userGuid !== id),
		data: x,
		label: (
			<>
				<Tag>{x.email}</Tag>
				{x.fullName}&ensp;
				{x.userGuid ? (
					x.userGuid === id ? (
						<Tag color='green'>текущий</Tag>
					) : (
						<Tag color='warning'>уже добавлен</Tag>
					)
				) : null}
			</>
		),
		// всё, по чему хочешь искать — в одну строку
		search: `${x.fullName ?? ''} ${x.email ?? ''} ${x.energoIdGuid ?? ''}`.toLowerCase(),
	}))

	return isLoading && !userData ? (
		<Spin />
	) : userData ? (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.users.list)}>Вернуться</Button>]}
				right={[
					<Popconfirm
						title='Вы уверены, что хотите удалить эту учётную запись?'
						placement='bottom'
						onConfirm={del}
						okText='Да'
						cancelText='Нет'
					>
						<Button>Удалить</Button>
					</Popconfirm>,
					<Button type='primary' onClick={update}>
						Сохранить
					</Button>,
				]}
				icon={<UserIcon />}
			>
				Учётная запись: {oldName}
			</PageHeader>
			<form>
				<FormRow title='Уровень глобального доступа'>
					<Select
						value={request.accessType}
						defaultValue={request.accessType}
						options={accessOptions.filter((x) => hasAccess(store.getGlobalAccess(), x.value as AccessType))}
						style={{ width: '12em' }}
						disabled={!store.hasGlobalAccess(AccessType.Manager)}
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
						value={newType}
						disabled={!hasAccess(store.getGlobalAccess(), AccessType.Manager)}
						onChange={(e) => setNewType(e.target.value)}
					>
						<Radio.Button value={UserType.Local}>Базовая учетная запись</Radio.Button>
						<Radio.Button value={UserType.EnergoId}>Учетная запись EnergoID</Radio.Button>
					</Radio.Group>
				</FormRow>

				<div
					style={{
						display: newType === UserType.Local ? 'inherit' : 'none',
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
						display: newType === UserType.Local ? 'inherit' : 'none',
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
						display: newType === UserType.Local ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Пароль'>
						<Input.Password
							value={request.password || ''}
							autoComplete='password'
							placeholder={
								userInfo.oldType !== UserType.Local
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
						display: newType === UserType.EnergoId ? 'inherit' : 'none',
					}}
				>
					<FormRow title='Учетная запись на сервере EnergoID'>
						<Select<EnergoIdOption>
							showSearch
							value={options.find((o) => o.value === request.energoIdGuid) ?? null}
							placeholder='Укажите учетную запись EnergoID'
							options={options}
							optionFilterProp='search'
							filterOption={(input, option) => (option?.search ?? '').includes(input.toLowerCase())}
							optionRender={(opt) => opt.label}
							onChange={(_, option) => {
								const typedOption = option as EnergoIdOption
								setRequest({
									...request,
									energoIdGuid: typedOption.value,
									login: typedOption.data.email,
									fullName: typedOption.data.fullName,
								})
							}}
							disabled={!hasAccess(store.getGlobalAccess(), AccessType.Manager)}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
			</form>
		</>
	) : null
})

export default UserForm
