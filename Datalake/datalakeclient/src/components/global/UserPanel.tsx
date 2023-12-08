import { Button } from "antd"
import { auth } from "../../etc/auth"
import axios from "axios"
import { API } from "../../router/api"
import { useNavigate } from "react-router-dom"

const style = {
	marginTop: '1em',
	padding: '0 1em',
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center'
}

export default function UserPanel () {

	const navigate = useNavigate()

	var token = Number(auth.token())

	function logout() {
		axios
			.post(API.auth.logout, { token: token })
			.then(res => res.data.Done && navigate('/login'))
	}

	return (token > 0)
		? <div style={style}>
			<div>Вы зашли как <b style={{ fontWeight: '500', color: '#0074d5' }}>{String(auth.name())}</b></div>
			<Button onClick={logout}>Выход</Button>
		</div>
		: <div>Не авторизован</div>
}