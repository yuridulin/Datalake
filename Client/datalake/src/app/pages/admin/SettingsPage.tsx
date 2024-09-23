import { Button, Form, Input } from 'antd'
import { useEffect } from 'react'
import api from '../../../api/swagger-api'
import { SettingsInfo } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'

const SettingsPage = () => {
	const [form] = Form.useForm<SettingsInfo>()

	const load = () => {
		api.configGetSettings().then((res) => form.setFieldsValue(res.data))
	}

	useEffect(load, [form])

	return (
		<>
			<Header
				right={
					<Button type='primary' onClick={form.submit}>
						Сохранить
					</Button>
				}
			>
				Настройки
			</Header>
			<Form
				form={form}
				layout='vertical'
				onFinish={api.configUpdateSettings}
			>
				<Form.Item<SettingsInfo>
					label='Адрес Keycloak сервера EnergoId'
					name='energoIdHost'
				>
					<Input placeholder='auth.energo.net' prefix='https://' />
				</Form.Item>
				<Form.Item<SettingsInfo>
					label='Адрес EnergoId'
					name='energoIdClient'
				>
					<Input placeholder='datalake' />
				</Form.Item>
				<Form.Item<SettingsInfo>
					label='Путь к API EnergoId'
					name='energoIdApi'
				>
					<Input
						placeholder='api.auth.energo.net/api/v1/users'
						prefix='https://'
					/>
				</Form.Item>
			</Form>
		</>
	)
}

export default SettingsPage
