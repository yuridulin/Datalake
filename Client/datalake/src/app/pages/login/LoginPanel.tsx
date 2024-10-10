import { Button, Form, Input, Space } from 'antd'
import { useAuth } from 'react-oidc-context'
import { useNavigate } from 'react-router-dom'
import { setName } from '../../../api/local-auth'
import notify from '../../../api/notifications'
import api from '../../../api/swagger-api'
import { UserLoginPass } from '../../../api/swagger/data-contracts'
import { useUpdateContext } from '../../../context/updateContext'
import routes from '../../router/routes'

export default function LoginPanel() {
	const navigate = useNavigate()
	const auth = useAuth()
	const { isDarkMode } = useUpdateContext()

	const style = {
		width: '40em',
		padding: '5em',
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const onFinish = (values: any) => {
		api.usersAuthenticate({
			login: values.login,
			password: values.password,
		})
			.then((res) => {
				if (res.status === 200) {
					setName(res.data.fullName)
					navigate(routes.globalRoot)
				}
			})
			.catch(() => {
				notify.err('Аутентификация не пройдена')
				navigate(routes.auth.loginPage)
			})
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const onFinishFailed = (errorInfo: any) => {
		console.log('Failed:', errorInfo)
	}

	return (
		<div
			style={{
				position: 'fixed',
				top: 0,
				left: 0,
				right: 0,
				bottom: 0,
				backgroundColor: isDarkMode ? '#121212' : '#fff',
			}}
		>
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
						rules={[
							{ required: true, message: 'Пароль не введён' },
						]}
					>
						<Input.Password
							name='password'
							autoComplete='password'
						/>
					</Form.Item>

					<Form.Item wrapperCol={{ offset: 8, span: 16 }}>
						<Space>
							<Button type='primary' htmlType='submit'>
								Вход
							</Button>
							<Button onClick={() => void auth.signinRedirect()}>
								Вход через EnergoID
							</Button>
						</Space>
					</Form.Item>
				</Form>
			</div>
		</div>
	)
}
