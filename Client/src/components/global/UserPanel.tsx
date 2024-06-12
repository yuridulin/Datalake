import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { Navigate, useNavigate } from 'react-router-dom'
import { auth } from '../../api/auth'
import routes from '../../router/routes'

const style = {
	marginTop: '1em',
	padding: '0 1em',
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center',
}

const UserPanel = () => {
	const navigate = useNavigate()

	function logout() {
		navigate(routes.Auth.LoginPage)
	}

	return auth.isAuthenticated ? (
		<div style={style}>
			<div>
				<b style={{ fontWeight: '500', color: '#33a2ff' }}>
					{auth.fullName}
				</b>
			</div>
			<Button onClick={logout}>Выход</Button>
		</div>
	) : (
		<Navigate to={routes.Auth.LoginPage} />
	)
}

export default observer(UserPanel)
