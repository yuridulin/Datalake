import { Tabs } from 'antd'
import PageHeader from '../../components/PageHeader'
import AuthSettings from './tabs/AuthSettings'
import CollectSettings from './tabs/CollectSettings'

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
				]}
			/>
		</>
	)
}

export default SettingsPage
