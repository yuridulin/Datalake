import { Button, Form, Input } from "antd"
import axios from "axios"
import { useNavigate } from "react-router-dom"
import { API } from "../../router/api"

type FieldType = {
	username?: string
	password?: string
}

const style = {
	width: '40em',
	padding: '5em',
}

export default function LoginPanel () {

	const navigate = useNavigate()

	const onFinish = (values: any) => {
		console.log(values)
		axios.post(API.auth.login, { name: values.username, password: values.password })
			.then(res => {
				if (res.data.Done) {
					navigate('/')
				}
			})
	}
	
	const onFinishFailed = (errorInfo: any) => {
		console.log('Failed:', errorInfo)
	}

	return <div style={style}>
		<Form
			name="basic"
			labelCol={{ span: 8 }}
			wrapperCol={{ span: 16 }}
			style={{ maxWidth: 600 }}
			onFinish={onFinish}
			onFinishFailed={onFinishFailed}
			autoComplete="off"
		>
				<Form.Item<FieldType>
					label="Имя учётной записи"
					name="username"
					rules={[{ required: true, message: 'Имя не введено' }]}
				>
					<Input name="username" autoComplete="username" />
				</Form.Item>

				<Form.Item<FieldType>
					label="Пароль"
					name="password"
					rules={[{ required: true, message: 'Пароль не введён' }]}
				>
					<Input.Password name="password" autoComplete="password" />
				</Form.Item>

				<Form.Item wrapperCol={{ offset: 8, span: 16 }}>
					<Button type="primary" htmlType="submit">Вход</Button>
				</Form.Item>
		</Form>
	</div>
}