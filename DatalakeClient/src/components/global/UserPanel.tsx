import { Button } from 'antd'
import { useNavigate } from 'react-router-dom'
import { auth } from '../../etc/auth'

const style = {
	marginTop: '1em',
	padding: '0 1em',
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center',
}

export default function UserPanel() {
	const navigate = useNavigate()
	var token = Number(auth.token())

	function logout() {
		navigate('/login')
	}

	return token > 0 ? (
		<div style={style}>
			<div>
				Вы зашли как{' '}
				<b style={{ fontWeight: '500', color: '#33a2ff' }}>
					{String(auth.name())}
				</b>
			</div>
			<Button onClick={logout}>Выход</Button>
		</div>
	) : (
		<div>Не авторизован</div>
	)
}
