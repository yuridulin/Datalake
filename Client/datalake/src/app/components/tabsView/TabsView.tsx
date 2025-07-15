import { Tabs, TabsProps } from 'antd'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

type TabsViewProps = {
	items: TabsProps['items']
}

const TabsView = ({ items }: TabsViewProps) => {
	const [activeTab, setActiveTab] = useState<string>()
	const [searchParams, setSearchParams] = useSearchParams()

	useEffect(() => console.log(searchParams), [searchParams])

	const onTabChange = (key: string) => {
		searchParams.set('page', key)
		setSearchParams(searchParams)
	}

	useEffect(() => {
		const tabFromQuery = searchParams.get('page')
		if (tabFromQuery === activeTab) return

		if (tabFromQuery) setActiveTab(tabFromQuery)
		else if (items?.length) setActiveTab(items[0].key)
	}, [searchParams, activeTab, items])

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
