import { UserLoginPass } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Form, Input, Space } from 'antd'
import { observer } from 'mobx-react-lite'
import { ValidateErrorEntity } from 'node_modules/rc-field-form/lib/interface'
import { Navigate } from 'react-router-dom'
import routes from '../routes'
import { handleKeycloakLogin } from './keycloak/oidcConfig'

const style = {
	width: '40em',
	padding: '5em',
}

const Login = observer(() => {
	const store = useAppStore()

	const onFinish = (values: UserLoginPass) => {
		store.loginLocal(values.login, values.password)
	}

	const onFinishFailed = (errorInfo: ValidateErrorEntity<UserLoginPass>) => {
		console.warn('Failed:', errorInfo)
	}

	return store.isAuthenticated ? (
		<Navigate to={routes.globalRoot} replace />
	) : (
		<div
			style={{
				position: 'fixed',
				top: 0,
				left: 0,
				right: 0,
				bottom: 0,
				backgroundColor: store.isDark ? '#121212' : '#fff',
			}}
		>
			<div style={style}>
				<Form<UserLoginPass>
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
							<Button onClick={handleKeycloakLogin}>Вход через EnergoID</Button>
						</Space>
					</Form.Item>
				</Form>
			</div>
		</div>
	)
})

export default Login
