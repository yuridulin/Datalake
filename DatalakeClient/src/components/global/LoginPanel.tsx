import { Button, Form, Input } from 'antd'
import { useNavigate } from 'react-router-dom'
import api from '../../api/api'
import { UserLoginPass } from '../../api/swagger/data-contracts'

const style = {
	width: '40em',
	padding: '5em',
}

export default function LoginPanel() {
	const navigate = useNavigate()

	const onFinish = (values: any) => {
		api.usersAuthenticate({
			name: values.username,
			password: values.password,
		}).then((res) => {
			if (res.status === 200) {
				navigate('/')
			}
		})
	}

	const onFinishFailed = (errorInfo: any) => {
		console.log('Failed:', errorInfo)
	}

	return (
		<div style={style}>
			<Form
				name='basic'
				labelCol={{ span: 8 }}
				wrapperCol={{ span: 16 }}
				style={{ maxWidth: 600 }}
				onFinish={onFinish}
				onFinishFailed={onFinishFailed}
				autoComplete='off'
			>
				<Form.Item<UserLoginPass>
					label='Имя учётной записи'
					name='username'
					rules={[{ required: true, message: 'Имя не введено' }]}
				>
					<Input name='username' autoComplete='username' />
				</Form.Item>

				<Form.Item<UserLoginPass>
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
		</div>
	)
}
