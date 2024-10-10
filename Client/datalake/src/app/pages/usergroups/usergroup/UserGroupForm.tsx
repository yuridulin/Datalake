import {
	MinusCircleOutlined,
	PlusOutlined,
	TeamOutlined,
} from '@ant-design/icons'
import {
	Button,
	Form,
	Input,
	Popconfirm,
	Select,
	SelectProps,
	Space,
	Spin,
} from 'antd'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import notify from '../../../../api/notifications'
import api from '../../../../api/swagger-api'
import {
	AccessType,
	UserGroupDetailedInfo,
	UserGroupUpdateRequest,
} from '../../../../api/swagger/data-contracts'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

const accessOptions: SelectProps['options'] = [
	{
		value: AccessType.NotSet,
		label: 'не определено',
	},
	{
		value: AccessType.NoAccess,
		label: 'нет доступа',
	},
	{
		value: AccessType.Viewer,
		label: 'просмотр',
	},
	{
		value: AccessType.User,
		label: 'редактирование',
	},
	{
		value: AccessType.Admin,
		label: 'полный доступ',
	},
]

export default function UserGroupForm() {
	const navigate = useNavigate()
	const { id } = useParams()
	const [form] = Form.useForm<UserGroupUpdateRequest>()

	const [ready, setReady] = useState(false)
	const [group, setGroup] = useState({} as UserGroupDetailedInfo)
	const [users, setUsers] = useState([] as { label: string; value: string }[])

	const getGroup = (guid: string) => {
		api.userGroupsReadWithDetails(guid).then((res) => {
			if (res.data.guid) {
				setGroup(res.data)
				form.setFieldsValue({
					name: res.data.name,
					description: res.data.description,
					parentGuid: null,
					accessType: res.data.globalAccessType,
					users: res.data.users,
				} as UserGroupUpdateRequest)
			}
		})
	}

	const updateGroup = (newInfo: UserGroupUpdateRequest) => {
		api.userGroupsUpdate(String(id), newInfo).catch(() => {
			notify.err('Ошибка при сохранении')
		})
	}

	const deleteGroup = () => {
		api.userGroupsDelete(String(id)).then(() =>
			navigate(routes.userGroups.toList()),
		)
	}

	const getReady = () => setReady(!!group.guid)

	const getUsers = () => {
		api.usersReadAll().then((res) => {
			if (res.data)
				setUsers(
					res.data.map((x) => ({
						value: x.guid,
						label: x.fullName || '?',
					})),
				)
		})
	}

	const load = () => {
		getGroup(String(id))
		getUsers()
	}

	const filterUserOption = (
		input: string,
		option?: { label: string; value: string },
	) => (option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(load, [id])
	useEffect(getReady, [group, users])

	return !ready ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.userGroups.toUserGroup(String(id))}>
						<Button>Вернуться</Button>
					</NavLink>
				}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить эту группу?'
							placement='bottom'
							onConfirm={deleteGroup}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						&ensp;
						<Button type='primary' onClick={() => form.submit()}>
							Сохранить
						</Button>
					</>
				}
			>
				<Space>
					<TeamOutlined style={{ fontSize: '20px' }} /> {group.name}
				</Space>
			</PageHeader>

			<Form form={form} onFinish={updateGroup} labelCol={{ span: 8 }}>
				<Form.Item<UserGroupUpdateRequest> label='Название' name='name'>
					<Input placeholder='Введите имя группы пользователей' />
				</Form.Item>
				<Form.Item<UserGroupUpdateRequest>
					label='Описание'
					name='description'
				>
					<Input.TextArea
						placeholder='Введите описание группы пользователей'
						autoSize={{ minRows: 2, maxRows: 8 }}
					/>
				</Form.Item>
				<Form.Item<UserGroupUpdateRequest>
					label='Базовый уровень доступа группы'
					name='accessType'
				>
					<Select
						placeholder='Выберите уровень доступа'
						options={accessOptions}
					/>
				</Form.Item>
				<Form.List name='users'>
					{(fields, { add, remove }) => (
						<table className='form-subtable'>
							<thead>
								<tr>
									<td>Пользователь</td>
									<td style={{ width: '20em' }}>
										Уровень доступа
									</td>
									<td style={{ width: '3em' }}>
										<Form.Item>
											<Button
												title='Добавить новое значение'
												onClick={() =>
													add({
														accessType:
															AccessType.NotSet,
													})
												}
											>
												<PlusOutlined />
											</Button>
										</Form.Item>
									</td>
								</tr>
							</thead>
							<tbody>
								{fields.map(({ key, name, ...rest }) => (
									<tr key={key}>
										<td>
											<Form.Item
												{...rest}
												name={[name, 'guid']}
											>
												<Select
													showSearch
													optionFilterProp='children'
													filterOption={
														filterUserOption
													}
													options={users}
													placeholder='Выберите учетную запись'
												/>
											</Form.Item>
										</td>
										<td>
											<Form.Item
												{...rest}
												name={[name, 'accessType']}
											>
												<Select
													options={accessOptions}
													placeholder='Выберите уровень доступа'
												></Select>
											</Form.Item>
										</td>
										<td>
											<Form.Item>
												<Button
													onClick={() => remove(name)}
													title='Удалить значение'
												>
													<MinusCircleOutlined />
												</Button>
											</Form.Item>
										</td>
									</tr>
								))}
							</tbody>
						</table>
					)}
				</Form.List>
			</Form>
		</>
	)
}
