import AccessTypeEl from '@/app/components/AccessTypeEl'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import { AccessRightsIdInfo, AccessType, BlockSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { accessOptions } from '@/types/accessOptions'
import { MinusOutlined, PlusOutlined } from '@ant-design/icons'
import { Button, Select, Spin, Table } from 'antd'
import { DefaultOptionType } from 'antd/es/select'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'

type FormType = AccessRightsIdInfo & {
	key: string
	choosedObject: 'group' | 'user'
}

const objectOptions: DefaultOptionType[] = [
	{
		value: 'group',
		label: 'Группа пользователей',
	},
	{
		value: 'user',
		label: 'Пользователь',
	},
]

const BlockAccessForm = () => {
	const store = useAppStore()
	const { id } = useParams()

	const [block, setBlock] = useState({} as BlockSimpleInfo)
	const [form, setForm] = useState([] as FormType[])
	const [err, setErr] = useState(null as string | null)
	const [loading, setLoading] = useState(false)
	const [userGroups, setUserGroups] = useState([] as DefaultOptionType[])
	const [users, setUsers] = useState([] as DefaultOptionType[])

	const getRights = () => {
		if (!id) return
		setLoading(true)
		const loaders = [
			store.api.blocksGet(Number(id)).then((res) => {
				setBlock(res.data)
			}),
			store.api
				.userGroupsGetAll()
				.then((res) => setUserGroups(res.data.map((x) => ({ label: x.name, value: x.guid })))),
			store.api.usersGetAll().then((res) =>
				setUsers(
					res.data.map((x) => ({
						label: x.fullName,
						value: x.guid,
					})),
				),
			),
			store.api
				.accessGet({ block: Number(id) })
				.then((res) => {
					setForm(
						res.data
							.filter((x) => !x.isGlobal)
							.map((x) => ({
								userGroupGuid: x.userGroup?.guid ?? null,
								userGuid: x.user?.guid ?? null,
								accessType: x.accessType,
								key: String(x.id),
								choosedObject: x.userGroup != null ? 'group' : 'user',
							})),
					)
				})
				.catch((e) => setErr(e)),
		]
		Promise.all(loaders).finally(() => setLoading(false))
	}

	const updateRights = () => {
		store.api.accessApplyChanges({
			blockId: Number(id),
			rights: form.map((x) => {
				switch (x.choosedObject) {
					case 'group':
						return { ...x, userGuid: null }

					case 'user':
						return { ...x, userGroupGuid: null }
				}
			}),
		})
	}

	useEffect(getRights, [id])

	const columns: ColumnsType<FormType> = [
		{
			title: () => {
				return (
					<Button
						onClick={() => {
							setForm([
								...form,
								{
									key: String(Date.now()),
									choosedObject: 'group',
									accessType: AccessType.NotSet,
								},
							])
						}}
					>
						<PlusOutlined />
					</Button>
				)
			},
			width: '5em',
			render: (_, record) => {
				return (
					<Button
						onClick={() => {
							setForm(form.filter((x) => x.key !== record.key))
						}}
					>
						<MinusOutlined />
					</Button>
				)
			},
		},
		{
			title: 'Уровень доступа',
			width: '14em',
			sorter: (a, b) => Number(a.accessType) - Number(b.accessType),
			render: (_, record) => {
				return (
					<Select
						options={accessOptions}
						style={{ width: '100%' }}
						value={record.accessType}
						onChange={(value) => {
							setForm(form.map((x) => (x.key === record.key ? { ...x, accessType: value } : x)))
						}}
						labelRender={(x) => <AccessTypeEl type={x.value as AccessType} />}
					/>
				)
			},
		},
		{
			title: 'Тип объекта',
			width: '10em',
			sorter: (a, b) => a.choosedObject.localeCompare(b.choosedObject),
			render: (_, record) => {
				return (
					<Select
						style={{ width: '100%' }}
						options={objectOptions}
						value={record.choosedObject}
						onChange={(value) => {
							setForm(
								form.map((x) =>
									x.key === record.key
										? {
												...x,
												choosedObject: value,
											}
										: x,
								),
							)
						}}
					/>
				)
			},
		},
		{
			title: 'Объект',
			render: (_, record) => {
				switch (record.choosedObject) {
					case 'group':
						return (
							<Select
								options={userGroups}
								style={{ width: '100%' }}
								value={record.userGroupGuid}
								onChange={(value) => {
									setForm(
										form.map((x) =>
											x.key === record.key
												? {
														...x,
														userGroupGuid: value,
													}
												: x,
										),
									)
								}}
							></Select>
						)
					case 'user':
						return (
							<Select
								options={users}
								value={record.userGuid}
								style={{ width: '100%' }}
								onChange={(value) => {
									setForm(
										form.map((x) =>
											x.key === record.key
												? {
														...x,
														userGuid: value,
													}
												: x,
										),
									)
								}}
							></Select>
						)
				}
			},
		},
	]

	return loading ? (
		<Spin />
	) : err ? (
		<>{err}</>
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.blocks.toViewBlock(Number(id))}>
						<Button>К просмотру блока</Button>
					</NavLink>
				}
				right={
					<Button type='primary' onClick={updateRights}>
						Сохранить
					</Button>
				}
			>
				Разрешения на доступ к блоку "{block.name}"
			</PageHeader>
			<Table size='small' pagination={false} columns={columns} dataSource={form} rowKey='key' />
		</>
	)
}

export default BlockAccessForm
