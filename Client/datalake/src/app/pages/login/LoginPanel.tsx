import { Button, Form, Input, Space } from 'antd'
import { useNavigate } from 'react-router-dom'
import { keycloak } from '../../../api/keycloak'
import api from '../../../api/swagger-api'
import { UserLoginPass } from '../../../api/swagger/data-contracts'
import routes from '../../router/routes'

const style = {
	width: '40em',
	padding: '5em',
}

export default function LoginPanel() {
	const navigate = useNavigate()

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const onFinish = (values: any) => {
		api.usersAuthenticate({
			login: values.login,
			password: values.password,
		}).then((res) => {
			console.log(res, res && res.status < 400)
			if (res && res.status < 400) navigate('/')
		})
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
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
				autoComplete='on'
			>
				<Form.Item<UserLoginPass>
					label='Имя учётной записи'
					name='login'
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
					<Space>
						<Button type='primary' htmlType='submit'>
							Вход
						</Button>
						<Button
							onClick={() =>
								keycloak.init({
									onLoad: 'login-required',
									redirectUri:
										window.location.origin +
										routes.Auth.EnergoId,
								})
							}
						>
							Вход через EnergoID
						</Button>
					</Space>
				</Form.Item>
			</Form>
		</div>
	)
}
