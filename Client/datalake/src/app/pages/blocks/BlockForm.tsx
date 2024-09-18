import {
	CreditCardOutlined,
	MinusCircleOutlined,
	PlusOutlined,
} from '@ant-design/icons'
import {
	Button,
	Form,
	Input,
	notification,
	Popconfirm,
	Select,
	Space,
} from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { BlockInfo } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'
import routes from '../../router/routes'
import styles from './BlockForm.module.css'

export default function BlockForm() {
	const { id } = useParams()
	const navigate = useNavigate()
	const [form] = Form.useForm<BlockInfo>()

	const [block, setBlock] = useState({} as BlockInfo)
	const [tags, setTags] = useState([] as { label: string; value: string }[])

	const getBlock = () => {
		api.blocksRead(Number(id)).then((res) => {
			setBlock(res.data)
			form.setFieldsValue(res.data)
		})
	}

	const updateBlock = (newInfo: BlockInfo) => {
		api.blocksUpdate(Number(id), newInfo)
			.then(() => {
				setBlock(newInfo)
			})
			.catch(() => {
				notification.error({
					message: 'Ошибка при сохранении',
				})
			})
	}

	const deleteBlock = () => {
		api.blocksDelete(Number(id)).then(() => navigate(routes.Blocks.root))
	}

	const getTags = () => {
		api.tagsReadAll().then((res) => {
			setTags(
				res.data
					.map((x) => ({
						label: x.name,
						value: x.guid,
					}))
					.sort((a, b) => a.label.localeCompare(b.label)),
			)
		})
	}

	useEffect(getBlock, [id, form])
	useEffect(getTags, [])

	return (
		<>
			<Header
				left={
					<Button
						onClick={() =>
							navigate(routes.Blocks.routeToViewBlock(Number(id)))
						}
					>
						Вернуться
					</Button>
				}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить этот блок?'
							placement='bottom'
							onConfirm={deleteBlock}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						<Button type='primary' onClick={() => form.submit()}>
							Сохранить
						</Button>
					</>
				}
			>
				<Space>
					<CreditCardOutlined style={{ fontSize: '20px' }} />{' '}
					{block.name}
				</Space>
			</Header>

			<Form form={form} onFinish={updateBlock}>
				<Form.Item label='Название' name='name'>
					<Input placeholder='Введите простое имя блока' />
				</Form.Item>
				<Form.Item label='Описание' name='description'>
					<Input.TextArea
						placeholder='Описание блока'
						autoSize={{ minRows: 2, maxRows: 8 }}
					/>
				</Form.Item>
				<Form.List name='tags'>
					{(fields, { add, remove }) => (
						<table className={styles.tags}>
							<thead>
								<tr>
									<td>Значение блока</td>
									<td>Закрепленный тег</td>
									<td style={{ width: '1em' }}>
										<Form.Item>
											<Button
												onClick={() => add()}
												title='Добавить новое значение'
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
												name={[name, 'name']}
											>
												<Input placeholder='Введите имя значения в контексте блока' />
											</Form.Item>
										</td>
										<td>
											<Form.Item
												{...rest}
												name={[name, 'guid']}
											>
												<Select
													showSearch
													options={tags}
													placeholder='Выберите тег для прикрепления'
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
