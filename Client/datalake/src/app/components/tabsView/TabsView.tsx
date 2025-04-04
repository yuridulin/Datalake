import { Tabs, TabsProps } from 'antd'
import { useEffect, useState } from 'react'

type TabsViewProps = {
	items: TabsProps['items']
}

const TabsView = ({ items }: TabsViewProps) => {
	const [activeTab, setActiveTab] = useState<string>()

	const onTabChange = (key: string) => {
		setActiveTab(key)
		window.location.hash = key // Обновление хэша в URL
	}

	useEffect(() => {
		const hash = window.location.hash.replace('#', '')
		if (hash != activeTab) {
			if (!hash && items) setActiveTab(items[0].key)
			else setActiveTab(hash)
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [window.location.hash])

	useEffect(() => {
		const hash = window.location.hash.replace('#', '')
		if (hash) {
			setActiveTab(hash)
		}
	}, [])

	return (
		<Tabs
			activeKey={activeTab}
			onChange={onTabChange}
			destroyInactiveTabPane
			tabBarStyle={{ height: '100%' }}
			items={items}
		/>
	)
}

export default TabsView
