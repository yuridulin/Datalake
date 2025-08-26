import { SettingsInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Form, Input } from 'antd'
import { useEffect, useState } from 'react'

const LocalSettings = () => {
	const store = useAppStore()
	const [settings, setSettings] = useState({} as SettingsInfo)
	const [form] = Form.useForm<SettingsInfo>()

	const load = () => {
		store.api.systemGetSettings().then((res) => {
			setSettings(res.data)
			form.setFieldsValue(res.data)
		})
	}

	const update = (newSettings: SettingsInfo) => {
		store.api.systemUpdateSettings({ ...settings, ...newSettings }).then(() => {
			document.title = 'Datalake | ' + newSettings.instanceName
		})
	}

	useEffect(load, [store, form])

	return (
		<Form form={form} layout='vertical' onFinish={update}>
			<Form.Item<SettingsInfo> label='Название базы данных' name='instanceName'>
				<Input placeholder='Введите название базы данных' />
			</Form.Item>
			<Button type='primary' onClick={form.submit}>
				Сохранить
			</Button>
		</Form>
	)
}

export default LocalSettings
