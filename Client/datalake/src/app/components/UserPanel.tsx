/* eslint-disable react-refresh/only-export-components */
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Col, Row, theme } from 'antd'
import { observer } from 'mobx-react-lite'
import { Navigate, useNavigate } from 'react-router-dom'
import { freeToken, getName, isAuth } from '../../api/local-auth'
import routes from '../router/routes'

const UserPanel = () => {
	const navigate = useNavigate()
	const { token } = theme.useToken()

	function logout() {
		freeToken()
		navigate(routes.auth.loginPage)
	}

	return isAuth() ? (
		<Row align='middle' className='app-two-items'>
			<Col span={12}>
				<span
					style={{
						color: token.colorText,
					}}
				>
					<UserOutlined /> {getName()}
				</span>
			</Col>
			<Col span={12} style={{ textAlign: 'right' }}>
				<Button
					type='link'
					onClick={logout}
					title='Выход из учетной записи'
				>
					<LogoutOutlined />
				</Button>
			</Col>
		</Row>
	) : (
		<Navigate to={routes.auth.loginPage} />
	)
}

export default observer(UserPanel)
