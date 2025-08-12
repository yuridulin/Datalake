import api from '@/api/swagger-api'
import { Button, Form, Input, Modal } from 'antd'
import { useState } from 'react'
import { UserGroupCreateRequest } from '../../../../../api/swagger/data-contracts'

interface UserGroupsCreateModalProps {
	onCreate: () => void
	isSmall?: boolean
	parentGuid?: string
}

const UserGroupsCreateModal = ({ onCreate, parentGuid = undefined, isSmall = false }: UserGroupsCreateModalProps) => {
	const [open, setOpen] = useState(false)
	const [confirmLoading, setConfirmLoading] = useState(false)
	const [form] = Form.useForm()

	const showModal = () => setOpen(true)

	const handleOk = () => {
		setConfirmLoading(true)
		form.submit()
	}

	const finishForm = (formData: UserGroupCreateRequest) => {
		if (parentGuid) formData.parentGuid = parentGuid
		api
			.userGroupsCreate(formData)
			.then(() => {
				closeModal()
				onCreate()
			})
			.finally(() => setConfirmLoading(false))
	}

	const closeModal = () => {
		setOpen(false)
		setConfirmLoading(false)
		form.resetFields()
	}

	return (
		<>
			<Button size={isSmall ? 'small' : 'middle'} onClick={showModal}>
				Создать {parentGuid && 'дочернюю'} группу
			</Button>
			<Modal
				title='Создание группы пользователей'
				open={open}
				onOk={handleOk}
				confirmLoading={confirmLoading}
				onCancel={closeModal}
				okText='Создать'
				cancelText='Отмена'
			>
				<br />
				<Form
					name='userGroupCreate'
					layout='vertical'
					form={form}
					onFinish={finishForm}
					onFinishFailed={() => setConfirmLoading(false)}
				>
					<Form.Item<UserGroupCreateRequest>
						label='Наименование группы'
						name='name'
						rules={[
							{
								required: true,
								message: 'Необходимо ввести наименование',
							},
						]}
					>
						<Input />
					</Form.Item>

					<Form.Item<UserGroupCreateRequest> label='Описание' name='description'>
						<Input.TextArea />
					</Form.Item>
				</Form>
			</Modal>
		</>
	)
}

export default UserGroupsCreateModal
