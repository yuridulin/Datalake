import AccessTypeEl from '@/app/components/atomic/AccessTypeEl'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { Navigate, useNavigate } from 'react-router-dom'
import { user } from '../../state/user'
import routes from '../router/routes'

const UserPanel = observer(() => {
	const navigate = useNavigate()

	function logout() {
		user.logout()
		navigate(routes.auth.loginPage)
	}

	return user.isAuth() ? (
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
						{user.fullName}
					</td>
				</tr>
				<tr>
					<td style={{ padding: '.25em 0 .25em 1em', width: '1em' }}>
						<UserOutlined />
					</td>
					<td style={{ padding: '.25em 1em' }}>
						<AccessTypeEl type={user.globalAccessType} />
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
