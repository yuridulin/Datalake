import api from '@/api/swagger-api'
import { Button, Form, Input } from 'antd'
import { useEffect, useState } from 'react'
import { SettingsInfo } from '../../../../api/swagger/data-contracts'

const LocalSettings = () => {
	const [settings, setSettings] = useState({} as SettingsInfo)
	const [form] = Form.useForm<SettingsInfo>()

	const load = () => {
		api.systemGetSettings().then((res) => {
			setSettings(res.data)
			form.setFieldsValue(res.data)
		})
	}

	const update = (newSettings: SettingsInfo) => {
		api.systemUpdateSettings({ ...settings, ...newSettings }).then(() => {
			document.title = 'Datalake | ' + newSettings.instanceName
		})
	}

	useEffect(load, [form])

	return (
		<Form form={form} layout='vertical' onFinish={update}>
			<Form.Item<SettingsInfo>
				label='Название базы данных'
				name='instanceName'
			>
				<Input placeholder='Введите название базы данных' />
			</Form.Item>
			<Button type='primary' onClick={form.submit}>
				Сохранить
			</Button>
		</Form>
	)
}

export default LocalSettings
