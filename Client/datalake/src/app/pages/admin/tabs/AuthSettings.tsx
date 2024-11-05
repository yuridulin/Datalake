import api from '@/api/swagger-api'
import { Button, Form, Input } from 'antd'
import { useEffect, useState } from 'react'
import { SettingsInfo } from '../../../../api/swagger/data-contracts'

const AuthSettings = () => {
	const [settings, setSettings] = useState({} as SettingsInfo)
	const [form] = Form.useForm<SettingsInfo>()

	const load = () => {
		api.systemGetSettings().then((res) => {
			setSettings(res.data)
			form.setFieldsValue(res.data)
		})
	}
	const update = (newSettings: SettingsInfo) => {
		api.systemUpdateSettings({ ...settings, ...newSettings })
	}

	useEffect(load, [form])

	return (
		<Form form={form} layout='vertical' onFinish={update}>
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
			<Button type='primary' onClick={form.submit}>
				Сохранить
			</Button>
		</Form>
	)
}

export default AuthSettings
