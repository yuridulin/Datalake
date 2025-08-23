import { useAppStore } from '@/store/useAppStore'
import { Button, Form, Input, Layout } from 'antd'
import { observer } from 'mobx-react-lite'

type FieldType = {
	username: string
	password: string
}

export const NotAuthorized = observer(() => {
	const { login } = useAppStore()

	const onFinish = (values: FieldType) => {
		login(values.username, values.password)
	}

	const onFinishFailed = (errorInfo: unknown) => {
		console.log('Failed:', errorInfo)
	}

	return (
		<Layout>
			<Layout.Content style={{ height: '100vh', paddingTop: '2em' }}>
				<Form
					name='basic'
					labelCol={{ span: 8 }}
					wrapperCol={{ span: 16 }}
					style={{ maxWidth: 600 }}
					onFinish={onFinish}
					onFinishFailed={onFinishFailed}
					autoComplete='off'
				>
					<Form.Item<FieldType>
						label='Имя учётной записи'
						name='username'
						rules={[{ required: true, message: 'Имя не введено' }]}
					>
						<Input name='username' autoComplete='username' />
					</Form.Item>

					<Form.Item<FieldType>
						label='Пароль'
						name='password'
						rules={[{ required: true, message: 'Пароль не введён' }]}
					>
						<Input.Password name='password' autoComplete='password' />
					</Form.Item>

					<Form.Item wrapperCol={{ offset: 8, span: 16 }}>
						<Button type='primary' htmlType='submit'>
							Вход
						</Button>
					</Form.Item>
				</Form>
			</Layout.Content>
		</Layout>
	)
})
