import AccessSettings from '@/app/pages/admin/tabs/AccessSettings'
import { Tabs } from 'antd'
import PageHeader from '../../components/PageHeader'
import AuthSettings from './tabs/AuthSettings'
import CollectSettings from './tabs/CollectSettings'
import LocalSettings from './tabs/LocalSettings'

const SettingsPage = () => {
	return (
		<>
			<PageHeader>Настройки</PageHeader>
			<Tabs
				items={[
					{
						key: '1',
						label: 'EnergoId',
						children: <AuthSettings />,
					},
					{
						key: '2',
						label: 'Сбор данных',
						children: <CollectSettings />,
					},
					{
						key: '3',
						label: 'Общие настройки',
						children: <LocalSettings />,
					},
					{
						key: '4',
						label: 'Права доступа',
						children: <AccessSettings />,
					},
				]}
			/>
		</>
	)
}

export default SettingsPage
