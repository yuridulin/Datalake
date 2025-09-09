import PageHeader from '@/app/components/PageHeader'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { Tabs } from 'antd'
import AccessSettings from './tabs/AccessSettings'
import AuthSettings from './tabs/AuthSettings'
import CollectSettings from './tabs/CollectSettings'
import LocalSettings from './tabs/LocalSettings'

const SettingsPage = () => {
	useDatalakeTitle('Настройки')

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
