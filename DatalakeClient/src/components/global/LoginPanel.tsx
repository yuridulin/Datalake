import { Button, Form, Input, Space } from 'antd'
import { AxiosResponse } from 'axios'
import { useKeycloak } from 'keycloak-react-web'
import { useNavigate } from 'react-router-dom'
import api from '../../api/swagger-api'
import { UserLoginPass } from '../../api/swagger/data-contracts'

const style = {
	width: '40em',
	padding: '5em',
}

export default function LoginPanel() {
	const navigate = useNavigate()
	const { keycloak } = useKeycloak()

	const onFinish = (values: any) => {
		api.usersAuthenticate({
			login: values.username,
			password: values.password,
		}).then(onSuccessAuth)
	}

	const onSuccessAuth = (res: AxiosResponse) => {
		if (res.status === 200) {
			navigate('/')
		}
	}

	const onFinishFailed = (errorInfo: any) => {
		console.log('Failed:', errorInfo)
	}

	// после редиректа от keycloak
	if (keycloak.authenticated) {
		console.log('keycloak', keycloak.idTokenParsed)
		api.usersAuthenticateEnergoIdUser({
			keycloakGuid: keycloak.idTokenParsed?.sup,
			login: keycloak.idTokenParsed?.name,
			fullName: keycloak.idTokenParsed?.name,
		}).then(onSuccessAuth)
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
					<Space>
						<Button type='primary' htmlType='submit'>
							Вход
						</Button>
						<Button onClick={() => keycloak.login()}>
							Вход через EnergoID
						</Button>
					</Space>
				</Form.Item>
			</Form>
		</div>
	)
}
