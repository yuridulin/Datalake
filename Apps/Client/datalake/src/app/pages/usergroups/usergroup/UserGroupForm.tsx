import api from '@/api/swagger-api'
import hasAccess from '@/functions/hasAccess'
import { user } from '@/state/user'
import { accessOptions } from '@/types/accessOptions'
import { MinusCircleOutlined, PlusOutlined, TeamOutlined } from '@ant-design/icons'
import { Button, Form, Input, Popconfirm, Select, Space, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import { AccessType, UserGroupDetailedInfo, UserGroupUpdateRequest } from '../../../../api/swagger/data-contracts'
import notify from '../../../../state/notifications'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

const UserGroupForm = observer(() => {
	const navigate = useNavigate()
	const { id } = useParams()
	const [form] = Form.useForm<UserGroupUpdateRequest>()

	const [ready, setGety] = useState(false)
	const [group, setGroup] = useState({} as UserGroupDetailedInfo)
	const [users, setUsers] = useState([] as { label: string; value: string }[])

	const getGroup = (guid: string) => {
		api.userGroupsGetWithDetails(guid).then((res) => {
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
		api
			.userGroupsUpdate(String(id), {
				...newInfo,
				users: newInfo.users || group.users || [],
			})
			.catch(() => {
				notify.err('Ошибка при сохранении')
			})
	}

	const deleteGroup = () => {
		api.userGroupsDelete(String(id)).then(() => navigate(routes.userGroups.toList()))
	}

	const getGety = () => setGety(!!group.guid)

	const getUsers = () => {
		api.usersGetAll().then((res) => {
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

	const filterUserOption = (input: string, option?: { label: string; value: string }) =>
		(option?.label ?? '').toLowerCase().includes(input.toLowerCase())

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(load, [id])
	useEffect(getGety, [group, users])

	return !ready ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.userGroups.toViewUserGroup(String(id))}>
						<Button>Вернуться</Button>
					</NavLink>
				}
				right={
					<>
						{hasAccess(group.accessRule.access, AccessType.Editor) && (
							<Popconfirm
								title='Вы уверены, что хотите удалить эту группу?'
								placement='bottom'
								onConfirm={deleteGroup}
								okText='Да'
								cancelText='Нет'
							>
								<Button>Удалить</Button>
							</Popconfirm>
						)}
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
				<Form.Item<UserGroupUpdateRequest> label='Описание' name='description'>
					<Input.TextArea placeholder='Введите описание группы пользователей' autoSize={{ minRows: 2, maxRows: 8 }} />
				</Form.Item>
				<Form.Item<UserGroupUpdateRequest> label='Базовый уровень доступа группы' name='accessType'>
					<Select
						placeholder='Выберите уровень доступа'
						options={accessOptions.filter((x) => hasAccess(user.globalAccessType, x.value as AccessType))}
					/>
				</Form.Item>
				{user.hasAccessToGroup(AccessType.Manager, String(id)) && (
					<Form.List name='users'>
						{(fields, { add, remove }) => (
							<table className='form-subtable'>
								<thead>
									<tr>
										<td>Пользователь</td>
										<td style={{ width: '20em' }}>Уровень доступа</td>
										<td style={{ width: '3em' }}>
											<Form.Item>
												<Button
													title='Добавить новое значение'
													onClick={() =>
														add({
															accessType: AccessType.NotSet,
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
												<Form.Item {...rest} name={[name, 'guid']}>
													<Select
														showSearch
														optionFilterProp='children'
														filterOption={filterUserOption}
														options={users}
														placeholder='Выберите учетную запись'
													/>
												</Form.Item>
											</td>
											<td>
												<Form.Item {...rest} name={[name, 'accessType']}>
													<Select options={accessOptions} placeholder='Выберите уровень доступа'></Select>
												</Form.Item>
											</td>
											<td>
												<Form.Item>
													<Button onClick={() => remove(name)} title='Удалить значение'>
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
				)}
			</Form>
		</>
	)
})

export default UserGroupForm
