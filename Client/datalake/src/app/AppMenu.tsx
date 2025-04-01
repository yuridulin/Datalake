import AggregatedSourceIcon from '@/app/components/icons/AggregatedSourceIcon'
import CalculatedSourceIcon from '@/app/components/icons/CalculatedSourceIcon'
import ManualSourceIcon from '@/app/components/icons/ManualSourceIcon'
import UserGroupIcon from '@/app/components/icons/UserGroupIcon'
import UserIcon from '@/app/components/icons/UserIcon'
import { blue } from '@ant-design/colors'
import { EditOutlined, PlaySquareOutlined, SettingOutlined, UnorderedListOutlined } from '@ant-design/icons'
import { Menu, theme } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import { AccessType } from '../api/swagger/data-contracts'
import hasAccess from '../functions/hasAccess'
import { user } from '../state/user'
import BlockIcon from './components/icons/BlockIcon'
import SourceIcon from './components/icons/SourceIcon'
import TagIcon from './components/icons/TagIcon'
import routes from './router/routes'

type MenuType = 'item' | 'group' | 'divider'

const items = [
	{
		key: 'blocks',
		label: 'Блоки',
		type: 'group' as MenuType,
		minimalAccess: AccessType.NotSet,
		children: [
			{
				key: routes.blocks.list,
				label: (
					<NavLink to={routes.blocks.list}>
						<BlockIcon />
						&emsp;Дерево блоков
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
		],
	},
	{
		key: 'tags-group',
		type: 'group' as MenuType,
		label: 'Теги',
		minimalAccess: AccessType.NotSet,
		children: [
			{
				key: routes.tags.list,
				label: (
					<NavLink to={routes.tags.list}>
						<TagIcon type={1} />
						&emsp;Все теги
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: routes.tags.manual,
				label: (
					<NavLink to={routes.tags.manual}>
						<ManualSourceIcon />
						&emsp;Мануальные теги
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
			{
				key: routes.tags.calc,
				label: (
					<NavLink to={routes.tags.calc}>
						<CalculatedSourceIcon />
						&emsp;Вычисляемые теги
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
			{
				key: routes.tags.aggregated,
				label: (
					<NavLink to={routes.tags.aggregated}>
						<AggregatedSourceIcon />
						&emsp;Агрегатные теги
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
		],
	},
	{
		key: 'values',
		label: 'Значения',
		type: 'group' as MenuType,
		path: '',
		minimalAccess: AccessType.NotSet,
		children: [
			{
				key: routes.values.tagsViewer,
				label: (
					<NavLink to={routes.values.tagsViewer}>
						<PlaySquareOutlined style={{ color: blue[4] }} />
						&emsp;Просмотр
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: routes.values.tagsWriter,
				label: (
					<NavLink to={routes.values.tagsWriter}>
						<EditOutlined style={{ color: blue[4] }} />
						&emsp;Запись
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
		],
	},
	{
		key: 'sources',
		label: 'Источники данных',
		type: 'group' as MenuType,
		minimalAccess: AccessType.Viewer,
		children: [
			{
				key: routes.sources.list,
				label: (
					<NavLink to={routes.sources.list}>
						<SourceIcon />
						&emsp;Список источников
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
		],
	},
	{
		key: 'admin',
		label: 'Администрирование',
		type: 'group' as MenuType,
		minimalAccess: AccessType.NotSet,
		children: [
			{
				key: routes.stats.logs,
				label: (
					<NavLink to={routes.stats.logs}>
						<UnorderedListOutlined style={{ color: blue[4] }} />
						&emsp;Журнал
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: routes.users.list,
				label: (
					<NavLink to={routes.users.list}>
						<UserIcon />
						&emsp;Пользователи
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: routes.userGroups.list,
				label: (
					<NavLink to={routes.userGroups.list}>
						<UserGroupIcon />
						&emsp;Группы пользователей
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: routes.settings,
				label: (
					<NavLink to={routes.settings}>
						<SettingOutlined style={{ color: blue[5] }} />
						&emsp;Настройки
					</NavLink>
				),
				minimalAccess: AccessType.Admin,
			},
		],
	},
]

const AppMenu = observer(() => {
	const { token } = theme.useToken()
	const globalAccess = user.globalAccessType
	const [selected, setSelected] = useState([] as string[])

	const location = useLocation()
	const currentPath = location.pathname

	useEffect(() => {
		// Найти все ссылки с классом active
		const links = document.querySelectorAll('#app-menu a')
		const selectedKeys = [] as string[]
		links.forEach((link) => {
			const menuItem = link.closest('li')
			if (menuItem) {
				if (link.classList.contains('active')) {
					menuItem.classList.add('ant-menu-item-selected')
					selectedKeys.push((menuItem.getAttribute('data-menu-id') || '').replace('app-menu-', ''))
				} else menuItem.classList.remove('ant-menu-item-selected')
			}
		})
		setSelected(selectedKeys)
	}, [currentPath])

	const filteredItems: ItemType<MenuItemType>[] = items
		.filter((item) => hasAccess(globalAccess, item.minimalAccess))
		.map((item) => ({
			key: item.key,
			label: item.label,
			type: item.type,
			children: item.children
				.filter((child) => hasAccess(globalAccess, child.minimalAccess))
				.map((child) => ({ key: child.key, label: child.label })),
		}))

	return (
		<Menu
			style={{ border: 0, backgroundColor: token.colorBgLayout }}
			items={filteredItems}
			mode='inline'
			id='app-menu'
			selectedKeys={selected}
		/>
	)
})

export default AppMenu
