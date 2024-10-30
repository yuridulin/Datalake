import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { Navigate, useNavigate } from 'react-router-dom'
import { freeToken, isAuth, user } from '../../api/local-auth'
import routes from '../router/routes'
import AccessTypeEl from './AccessTypeEl'

const UserPanel = observer(() => {
	const navigate = useNavigate()

	function logout() {
		freeToken()
		navigate(routes.auth.loginPage)
	}

	return isAuth() ? (
		<table style={{ width: '100%', maxWidth: '100%' }}>
			<tbody>
				<tr>
					<td
						colSpan={2}
						style={{
							padding: '.25em 1em',
							wordBreak: 'break-word',
						}}
					>
						{user.name}
					</td>
				</tr>
				<tr>
					<td style={{ padding: '.25em 0 .25em 1em', width: '1em' }}>
						<UserOutlined />
					</td>
					<td style={{ padding: '.25em 1em' }}>
						<AccessTypeEl type={user.globalAccess} />
					</td>
					<td style={{ padding: '.25em 1em .25em 0', width: '1em' }}>
						<Button
							type='link'
							onClick={logout}
							title='Выход из учетной записи'
						>
							<LogoutOutlined />
						</Button>
					</td>
				</tr>
			</tbody>
		</table>
	) : (
		<Navigate to={routes.auth.loginPage} />
	)
})

export default UserPanel
