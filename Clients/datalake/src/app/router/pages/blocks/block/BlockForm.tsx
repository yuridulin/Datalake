import BlockIcon from '@/app/components/icons/BlockIcon'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import {
	AccessType,
	AttachedTag,
	BlockTagRelation,
	BlockUpdateRequest,
	SourceType,
	TagType,
} from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { MinusCircleOutlined, PlusOutlined } from '@ant-design/icons'
import { App, Button, Dropdown, Form, Input, Popconfirm, Select, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

const BlockForm = observer(() => {
	const store = useAppStore()
	const app = App.useApp()
	const { id } = useParams()
	useDatalakeTitle('Блоки', '#' + id, 'Изменение')

	const navigate = useNavigate()
	const [form] = Form.useForm<BlockUpdateRequest>()

	const blockId = id ? Number(id) : undefined
	// Получаем блок из store (реактивно через MobX)
	const blockData = blockId ? store.blocksStore.getBlockById(blockId) : undefined

	// Получаем теги из store (реактивно через MobX)
	const tagsData = store.tagsStore.getTags()
	const tags = useMemo(
		() =>
			tagsData
				.map((x) => ({
					...x,
					label: x.name,
					value: x.id,
				}))
				.sort((a, b) => a.label.localeCompare(b.label)),
		[tagsData],
	)

	const [block, setBlock] = useState({} as BlockUpdateRequest)
	const [loading, setLoading] = useState(true)

	// Обновляем локальное состояние блока при загрузке из store
	useEffect(() => {
		if (!blockData) {
			setLoading(true)
			return
		}

		const attachedTags = blockData.tags.map(
			(tag) =>
				({
					id: tag.tag?.id ?? tag.tagId ?? 0,
					name: tag.localName ?? tag.tag?.name ?? '',
					relation: tag.relationType,
				}) as AttachedTag,
		)
		const blockUpdate: BlockUpdateRequest = { ...blockData, tags: attachedTags }
		setBlock(blockUpdate)
		form.setFieldsValue(blockUpdate)
		setLoading(false)
	}, [blockData, form])

	const updateBlock = async (newInfo: BlockUpdateRequest) => {
		try {
			await store.api.inventoryBlocksUpdate(Number(id), newInfo)
			// Инвалидируем кэш и обновляем данные
			if (blockId) {
				store.blocksStore.invalidateBlock(blockId)
				await store.blocksStore.refreshBlocks()
			}
		} catch {
			app.notification.error({ message: 'Ошибка при сохранении' })
		}
	}

	const deleteBlock = async () => {
		try {
			await store.api.inventoryBlocksDelete(Number(id))
			// Инвалидируем кэш
			if (blockId) {
				store.blocksStore.invalidateBlock(blockId)
			}
			navigate(routes.blocks.root)
		} catch (error) {
			console.error('Failed to delete block:', error)
		}
	}

	// Получаем текущие значения формы для проверки дубликатов
	const attachedTagsList = Form.useWatch('tags', form) || []

	return loading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.blocks.toViewBlock(Number(id)))}>Вернуться</Button>]}
				right={[
					store.hasAccessToBlock(AccessType.Admin, Number(id)) && (
						<Popconfirm
							title='Вы уверены, что хотите удалить этот блок?'
							placement='bottom'
							onConfirm={deleteBlock}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
					),
					<Button type='primary' onClick={() => form.submit()}>
						Сохранить
					</Button>,
				]}
				icon={<BlockIcon />}
			>
				{block.name}
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
																store.api
																	.inventoryTagsCreate({
																		blockId: Number(id),
																		tagType: TagType.String,
																		sourceId: SourceType.Manual,
																	})
																	.then(async (res) => {
																		// Инвалидируем кэш тегов
																		store.tagsStore.invalidateTag(res.data.id)
																		await store.tagsStore.refreshTags()
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
																store.api
																	.inventoryTagsCreate({
																		blockId: Number(id),
																		tagType: TagType.Number,
																		sourceId: SourceType.Manual,
																	})
																	.then(async (res) => {
																		// Инвалидируем кэш тегов
																		store.tagsStore.invalidateTag(res.data.id)
																		await store.tagsStore.refreshTags()
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
																store.api
																	.inventoryTagsCreate({
																		blockId: Number(id),
																		tagType: TagType.Boolean,
																		sourceId: SourceType.Manual,
																	})
																	.then(async (res) => {
																		// Инвалидируем кэш тегов
																		store.tagsStore.invalidateTag(res.data.id)
																		await store.tagsStore.refreshTags()
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
