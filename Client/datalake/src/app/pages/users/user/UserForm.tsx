import api from '@/api/swagger-api'
import hasAccess from '@/functions/hasAccess'
import { user } from '@/state/user'
import { accessOptions } from '@/types/accessOptions'
import { Button, Input, Popconfirm, Radio, Select, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
	AccessType,
	EnergoIdInfo,
	UserDetailInfo,
	UserType,
	UserUpdateRequest,
} from '../../../../api/swagger/data-contracts'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

type UserInfoProps = UserDetailInfo & {
	oldType: UserType
	hash: string
}

const UserForm = observer(() => {
	const navigate = useNavigate()
	const { id } = useParams()
	const [oldName, setOldName] = useState('')
	const [userInfo, setUser] = useState({
		oldType: UserType.Local,
		hash: '',
	} as UserInfoProps)
	const [request, setRequest] = useState({} as UserUpdateRequest)
	const [newType, setNewType] = useState(UserType.Local)
	const [keycloakUsers, setKeycloakUsers] = useState({
		connected: false,
		energoIdUsers: [],
	} as EnergoIdInfo)
	const [loading, setLoading] = useState(true)

	const load = () => {
		if (!id) return
		setLoading(true)
		api.usersReadWithDetails(String(id)).then((res) => {
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

		api.usersGetEnergoIdList({ currentUserGuid: id }).then(
			(res) => !!res && setKeycloakUsers(res.data),
		)
	}

	useEffect(load, [id])

	function update() {
		api.usersUpdate(String(id), request).then((res) => {
			if (res.status >= 300) return
			load()
		})
	}

	function del() {
		api.usersDelete(String(id)).then(() => navigate(routes.users.list))
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

	return loading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<Button onClick={() => navigate(routes.users.list)}>
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
						&ensp;
						<Button type='primary' onClick={update}>
							Сохранить
						</Button>
					</>
				}
			>
				Учётная запись: {oldName}
			</PageHeader>
			<form>
				<FormRow title='Уровень глобального доступа'>
					<Select
						value={request.accessType}
						defaultValue={request.accessType}
						options={accessOptions.filter((x) =>
							hasAccess(
								user.globalAccessType,
								x.value as AccessType,
							),
						)}
						style={{ width: '12em' }}
						disabled={
							!hasAccess(
								userInfo.accessRule.accessType,
								AccessType.Manager,
							)
						}
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
						disabled={
							!hasAccess(
								user.globalAccessType,
								AccessType.Manager,
							)
						}
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
							newType === UserType.Local ? 'inherit' : 'none',
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
							newType === UserType.Local ||
							newType === UserType.Static
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
							user.hasGlobalAccess(AccessType.Admin) &&
							newType === UserType.Static
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
					{userInfo.oldType === UserType.Static ? (
						<FormRow title='Ключ для доступа'>
							<Input disabled value={userInfo.hash} />
							<div style={{ marginTop: '.5em' }}>
								<Button
									type='primary'
									onClick={() => {
										navigator.clipboard.writeText(
											userInfo.hash ?? '',
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
							options={keycloakUsers.energoIdUsers.map((x) => ({
								value: x.energoIdGuid,
								label: x.fullName + ' (' + x.login + ')',
							}))}
							disabled={
								!hasAccess(
									user.globalAccessType,
									AccessType.Manager,
								)
							}
							style={{ width: '100%' }}
							onChange={(value) => {
								const user = keycloakUsers.energoIdUsers.filter(
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
})

export default UserForm
