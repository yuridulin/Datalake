import api from '@/api/swagger-api'
import { accessOptions } from '@/types/accessOptions'
import { MinusOutlined, PlusOutlined } from '@ant-design/icons'
import { Button, Select, Spin, Table } from 'antd'
import { DefaultOptionType } from 'antd/es/select'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'
import {
	AccessRightsIdInfo,
	AccessType,
	UserGroupInfo,
} from '../../../../../api/swagger/data-contracts'
import AccessTypeEl from '../../../../components/AccessTypeEl'
import PageHeader from '../../../../components/PageHeader'
import routes from '../../../../router/routes'

type FormType = AccessRightsIdInfo & {
	key: string
	choosedObject: 'source' | 'block' | 'tag'
}

const objectOptions: DefaultOptionType[] = [
	{
		value: 'source',
		label: 'Источник',
	},
	{
		value: 'block',
		label: 'Блок',
	},
	{
		value: 'tag',
		label: 'Тег',
	},
]

const UserGroupAccessForm = () => {
	const { id } = useParams()

	const [group, setGroup] = useState({} as UserGroupInfo)
	const [form, setForm] = useState([] as FormType[])
	const [err, setErr] = useState(null as string | null)
	const [loading, setLoading] = useState(false)
	const [sources, setSources] = useState([] as DefaultOptionType[])
	const [blocks, setBlocks] = useState([] as DefaultOptionType[])
	const [tags, setTags] = useState([] as DefaultOptionType[])

	const getRights = () => {
		if (!id) return
		setLoading(true)
		const loaders = [
			api.userGroupsRead(String(id)).then((res) => {
				setGroup(res.data)
			}),
			api
				.sourcesReadAll()
				.then((res) =>
					setSources(
						res.data.map((x) => ({ label: x.name, value: x.id })),
					),
				),
			api
				.blocksReadAll()
				.then((res) =>
					setBlocks(
						res.data.map((x) => ({ label: x.name, value: x.id })),
					),
				),
			api
				.tagsReadAll()
				.then((res) =>
					setTags(
						res.data.map((x) => ({ label: x.name, value: x.id })),
					),
				),
			api
				.accessGet({ userGroup: String(id) })
				.then((res) => {
					setForm(
						res.data
							.filter((x) => !x.isGlobal)
							.map((x) => ({
								sourceId: x.source?.id ?? null,
								blockId: x.block?.id ?? null,
								tagId: x.tag?.id ?? null,
								accessType: x.accessType,
								key: String(x.id),
								choosedObject:
									x.source != null
										? 'source'
										: x.block != null
											? 'block'
											: 'tag',
							})),
					)
				})
				.catch((e) => setErr(e)),
		]
		Promise.all(loaders).finally(() => setLoading(false))
	}

	const updateRights = () => {
		api.accessApplyChanges({
			userGroupGuid: String(id),
			rights: form.map((x) => {
				switch (x.choosedObject) {
					case 'source':
						return { ...x, blockId: null, tagId: null }

					case 'block':
						return { ...x, sourceId: null, tagId: null }

					case 'tag':
						return { ...x, blockId: null, sourceId: null }
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
									choosedObject: 'block',
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
							setForm(
								form.map((x) =>
									x.key === record.key
										? { ...x, accessType: value }
										: x,
								),
							)
						}}
						labelRender={(x) => (
							<AccessTypeEl type={x.value as AccessType} />
						)}
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
					case 'source':
						return (
							<Select
								options={sources}
								style={{ width: '100%' }}
								value={record.sourceId}
								onChange={(value) => {
									setForm(
										form.map((x) =>
											x.key === record.key
												? {
														...x,
														sourceId: value,
													}
												: x,
										),
									)
								}}
							></Select>
						)
					case 'block':
						return (
							<Select
								options={blocks}
								value={record.blockId}
								style={{ width: '100%' }}
								onChange={(value) => {
									setForm(
										form.map((x) =>
											x.key === record.key
												? {
														...x,
														blockId: value,
													}
												: x,
										),
									)
								}}
							></Select>
						)
					case 'tag':
						return (
							<Select
								options={tags}
								value={record.tagId}
								style={{ width: '100%' }}
								onChange={(value) => {
									setForm(
										form.map((x) =>
											x.key === record.key
												? {
														...x,
														tagId: value,
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

	useEffect(() => console.log(form), [form])

	return loading ? (
		<Spin />
	) : err ? (
		<>{err}</>
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.userGroups.toUserGroup(String(id))}>
						<Button>К просмотру группы</Button>
					</NavLink>
				}
				right={
					<Button type='primary' onClick={updateRights}>
						Сохранить
					</Button>
				}
			>
				Разрешения, выдаваемые группе "{group.name}"
			</PageHeader>
			<Table
				size='small'
				pagination={false}
				columns={columns}
				dataSource={form}
				rowKey='key'
			/>
		</>
	)
}

export default UserGroupAccessForm
