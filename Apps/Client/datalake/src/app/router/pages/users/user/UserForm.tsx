import FormRow from '@/app/components/FormRow'
import UserIcon from '@/app/components/icons/UserIcon'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserDetailInfo, UserEnergoIdInfo, UserType, UserUpdateRequest } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { accessOptions } from '@/types/accessOptions'
import { Button, Input, Popconfirm, Radio, Select, Spin, Tag } from 'antd'
import { DefaultOptionType } from 'antd/es/select'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

type UserInfoProps = UserDetailInfo & {
	oldType: UserType
	hash: string
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
	const [oldName, setOldName] = useState('')
	const [userInfo, setUser] = useState({
		oldType: UserType.Local,
		hash: '',
	} as UserInfoProps)
	const [request, setRequest] = useState({} as UserUpdateRequest)
	const [newType, setNewType] = useState(UserType.Local)
	const [keycloakUsers, setKeycloakUsers] = useState<UserEnergoIdInfo[]>([])
	const [loading, setLoading] = useState(true)

	const load = () => {
		if (!id) return
		setLoading(true)
		store.api.usersGetWithDetails(String(id)).then((res) => {
			setNewType(res.data.type)
			setUser({
				...res.data,
				oldType: res.data.type,
				hash: res.data.hash ?? '',
			})
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
			setOldName(res.data.fullName ?? id)
			setLoading(false)
		})

		store.api.usersGetEnergoId().then((res) => !!res && setKeycloakUsers(res.data))
	}

	useEffect(load, [store.api, id])

	function update() {
		store.api.usersUpdate(String(id), request).then((res) => {
			if (res.status >= 300) return
			load()
		})
	}

	function del() {
		store.api.usersDelete(String(id)).then(() => navigate(routes.users.list))
	}

	function generateNewHash() {
		store.api
			.usersUpdate(String(id), {
				...request,
				createNewStaticHash: true,
			})
			.then(() => load())
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

	return loading ? (
		<Spin />
	) : (
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
						options={accessOptions.filter((x) => hasAccess(store.globalAccessType, x.value as AccessType))}
						style={{ width: '12em' }}
						disabled={!hasAccess(userInfo.accessRule.access, AccessType.Manager)}
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
						disabled={!hasAccess(store.globalAccessType, AccessType.Manager)}
						onChange={(e) => setNewType(e.target.value)}
					>
						<Radio.Button value={UserType.Static}>Статичная учетная запись</Radio.Button>
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
						display: newType === UserType.Local || newType === UserType.Static ? 'inherit' : 'none',
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
						display: store.hasGlobalAccess(AccessType.Admin) && newType === UserType.Static ? 'inherit' : 'none',
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
					{userInfo.oldType === UserType.Static ? (
						<FormRow title='Ключ для доступа'>
							<Input disabled value={userInfo.hash} />
							<div style={{ marginTop: '.5em' }}>
								<Button
									type='primary'
									onClick={() => {
										navigator.clipboard.writeText(userInfo.hash ?? '')
									}}
								>
									Скопировать
								</Button>
								&ensp;
								<Button onClick={generateNewHash}>Создать новый</Button>
							</div>
						</FormRow>
					) : (
						<></>
					)}
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
							disabled={!hasAccess(store.globalAccessType, AccessType.Manager)}
							style={{ width: '100%' }}
						/>
					</FormRow>
				</div>
			</form>
		</>
	)
})

export default UserForm
