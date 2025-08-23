import api from '@/api/swagger-api'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import {
	AccessType,
	AttachedTag,
	BlockTagRelation,
	BlockUpdateRequest,
	SourceType,
	TagResolution,
	TagSimpleInfo,
	TagType,
} from '@/generated/data-contracts'
import notify from '@/state/notifications'
import { user } from '@/state/user'
import { CreditCardOutlined, MinusCircleOutlined, PlusOutlined } from '@ant-design/icons'
import { Button, Dropdown, Form, Input, Popconfirm, Select, Space, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

interface TagInfo extends TagSimpleInfo {
	label: string
	value: number
}

const BlockForm = observer(() => {
	const { id } = useParams()
	const navigate = useNavigate()
	const [form] = Form.useForm<BlockUpdateRequest>()

	const [block, setBlock] = useState({} as BlockUpdateRequest)
	const [tags, setTags] = useState([] as TagInfo[])
	const [loading, setLoading] = useState(true)

	const getBlock = () => {
		api.blocksGet(Number(id)).then((res) => {
			const attachedTags = res.data.tags.map(
				(tag) =>
					({
						id: tag.id,
						name: tag.localName,
						relation: tag.relationType,
					}) as AttachedTag,
			)
			setBlock({ ...res.data, tags: attachedTags })
			form.setFieldsValue({
				...res.data,
				tags: attachedTags,
			} as BlockUpdateRequest)
		})
	}

	const updateBlock = (newInfo: BlockUpdateRequest) => {
		api.blocksUpdate(Number(id), newInfo).catch(() => {
			notify.err('Ошибка при сохранении')
		})
	}

	const deleteBlock = () => {
		api.blocksDelete(Number(id)).then(() => navigate(routes.blocks.root))
	}

	const getTags = () => {
		setLoading(true)
		api
			.tagsGetAll()
			.then((res) => {
				setTags(
					res.data
						.map((x) => ({
							...x,
							label: x.name,
							value: x.id,
						}))
						.sort((a, b) => a.label.localeCompare(b.label)),
				)
			})
			.finally(() => setLoading(false))
	}

	useEffect(getBlock, [id, form])
	useEffect(getTags, [])

	// Получаем текущие значения формы для проверки дубликатов
	const attachedTagsList = Form.useWatch('tags', form) || []

	return loading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={<Button onClick={() => navigate(routes.blocks.toViewBlock(Number(id)))}>Вернуться</Button>}
				right={
					<>
						{user.hasAccessToBlock(AccessType.Admin, Number(id)) && (
							<Popconfirm
								title='Вы уверены, что хотите удалить этот блок?'
								placement='bottom'
								onConfirm={deleteBlock}
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
					<CreditCardOutlined style={{ fontSize: '20px' }} /> {block.name}
				</Space>
			</PageHeader>

			<Form form={form} onFinish={updateBlock}>
				<Form.Item
					label='Название'
					name='name'
					rules={[
						{
							required: true,
							message: 'Название - обязательный параметр',
						},
					]}
				>
					<Input placeholder='Введите простое имя блока' />
				</Form.Item>
				<Form.Item label='Описание' name='description'>
					<Input.TextArea placeholder='Описание блока' autoSize={{ minRows: 2, maxRows: 8 }} />
				</Form.Item>
				<Form.List name='tags'>
					{(fields, { add, remove }) => (
						<table className='form-subtable'>
							<thead>
								<tr>
									<td style={{ width: '40%' }}>Поле блока</td>
									<td>Закрепленный тег</td>
									<td style={{ width: '3em' }}>
										<Form.Item>
											<Dropdown.Button
												title='Добавить новое поле'
												menu={{
													items: [
														{
															key: '1',
															label: 'Создать строковый мануальный тег и добавить как поле',
															onClick: () => {
																api
																	.tagsCreate({
																		blockId: Number(id),
																		tagType: TagType.String,
																		resolution: TagResolution.NotSet,
																		sourceId: SourceType.Manual,
																	})
																	.then((res) => {
																		setTags([
																			...tags,
																			{
																				...res.data,
																				label: res.data.name,
																				value: res.data.id,
																			},
																		])
																		add({
																			id: res.data.id,
																			name: res.data.name,
																			relation: BlockTagRelation.Static,
																		} as AttachedTag)
																	})
															},
														},
														{
															key: '2',
															label: 'Создать числовой мануальный тег и добавить как поле',
															onClick: () => {
																api
																	.tagsCreate({
																		blockId: Number(id),
																		tagType: TagType.Number,
																		resolution: TagResolution.NotSet,
																		sourceId: SourceType.Manual,
																	})
																	.then((res) => {
																		setTags([
																			...tags,
																			{
																				...res.data,
																				label: res.data.name,
																				value: res.data.id,
																			},
																		])
																		add({
																			id: res.data.id,
																			name: res.data.name,
																			relation: BlockTagRelation.Static,
																		} as AttachedTag)
																	})
															},
														},
														{
															key: '3',
															label: 'Создать логический мануальный тег и добавить как поле',
															onClick: () => {
																api
																	.tagsCreate({
																		blockId: Number(id),
																		tagType: TagType.Boolean,
																		resolution: TagResolution.NotSet,
																		sourceId: SourceType.Manual,
																	})
																	.then((res) => {
																		setTags([
																			...tags,
																			{
																				...res.data,
																				label: res.data.name,
																				value: res.data.id,
																			},
																		])
																		add({
																			id: res.data.id,
																			name: res.data.name,
																			relation: BlockTagRelation.Static,
																		} as AttachedTag)
																	})
															},
														},
													],
												}}
												onClick={() => add()}
											>
												<PlusOutlined />
											</Dropdown.Button>
										</Form.Item>
									</td>
								</tr>
							</thead>
							<tbody>
								{fields.map(({ key, name, ...rest }) => {
									// Проверка дублирования тегов
									const currentTagId = attachedTagsList[name]?.id
									const duplicateCount = currentTagId
										? attachedTagsList.filter((t: AttachedTag, idx: number) => idx !== name && t?.id === currentTagId)
												.length
										: 0
									const isDuplicate = duplicateCount > 0

									return (
										<tr key={key}>
											<td>
												<Form.Item
													{...rest}
													name={[name, 'name']}
													rules={[
														{
															required: true,
															message: 'Введите имя значения',
														},
														({ getFieldValue }) => ({
															validator(_, value) {
																if (!value) return Promise.resolve()
																const tags = getFieldValue('tags') || []
																const names = tags
																	.map((t: AttachedTag, index: number) => (index === name ? null : t.name))
																	.filter(Boolean)
																if (names.includes(value)) {
																	return Promise.reject(new Error('Имя поля должно быть уникальным'))
																}
																return Promise.resolve()
															},
														}),
													]}
												>
													<Input placeholder='Введите имя поля в контексте блока' />
												</Form.Item>
											</td>
											<td>
												<Form.Item
													{...rest}
													name={[name, 'id']}
													validateStatus={isDuplicate ? 'warning' : undefined}
													help={isDuplicate ? 'Дублирование тега' : undefined}
												>
													<Select
														showSearch
														optionFilterProp='label'
														options={tags}
														placeholder='Выберите тег для прикрепления'
													></Select>
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
									)
								})}
							</tbody>
						</table>
					)}
				</Form.List>
			</Form>
		</>
	)
})

export default BlockForm
