/* eslint-disable react-refresh/only-export-components */
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { Navigate, useNavigate } from 'react-router-dom'
import { freeToken, getName, isAuth } from '../../api/local-auth'
import routes from '../router/routes'

const style = {
	padding: '1em',
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center',
}

const UserPanel = () => {
	const navigate = useNavigate()

	function logout() {
		freeToken()
		navigate(routes.Auth.LoginPage)
	}

	return isAuth() ? (
		<div style={style}>
			<div
				style={{
					paddingLeft: '10px',
					fontWeight: '500',
					color: '#33a2ff',
				}}
			>
				{getName()}
			</div>
			<Button onClick={logout}>Выход</Button>
		</div>
	) : (
		<Navigate to={routes.Auth.LoginPage} />
	)
}

export default observer(UserPanel)
