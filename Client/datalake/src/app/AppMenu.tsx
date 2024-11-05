import UserGroupIcon from '@/app/components/icons/UserGroupIcon'
import UserIcon from '@/app/components/icons/UserIcon'
import { blue } from '@ant-design/colors'
import {
	CalculatorOutlined,
	EditOutlined,
	PlaySquareOutlined,
	SettingOutlined,
	UnorderedListOutlined,
} from '@ant-design/icons'
import { Menu, theme } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { observer } from 'mobx-react-lite'
import { NavLink } from 'react-router-dom'
import { AccessType } from '../api/swagger/data-contracts'
import hasAccess from '../functions/hasAccess'
import { user } from '../state/user'
import { CustomSource } from '../types/customSource'
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
				key: 'blocks-tree',
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
				key: 'tags',
				label: (
					<NavLink to={routes.tags.list}>
						<TagIcon />
						&emsp;Все теги
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: String(CustomSource.Manual),
				label: (
					<NavLink to={routes.tags.manual}>
						<EditOutlined style={{ color: blue[4] }} />
						&emsp;Мануальные теги
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
			{
				key: String(CustomSource.Calculated),
				label: (
					<NavLink to={routes.tags.calc}>
						<CalculatorOutlined style={{ color: blue[4] }} />
						&emsp;Вычисляемые теги
					</NavLink>
				),
				minimalAccess: AccessType.Viewer,
			},
		],
	},
	{
		key: 'viewer',
		label: 'Просмотр данных',
		type: 'group' as MenuType,
		minimalAccess: AccessType.NotSet,
		children: [
			{
				key: 'viewer-tags',
				label: (
					<NavLink to={routes.viewer.tagsViewer}>
						<PlaySquareOutlined style={{ color: blue[4] }} />
						&emsp;Запросы
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
				key: 'sources-list',
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
				key: 'logs',
				label: (
					<NavLink to={routes.stats.logs}>
						<UnorderedListOutlined style={{ color: blue[4] }} />
						&emsp;Журнал
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: 'users',
				label: (
					<NavLink to={routes.users.list}>
						<UserIcon />
						&emsp;Пользователи
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: 'user-groups',
				label: (
					<NavLink to={routes.userGroups.list}>
						<UserGroupIcon />
						&emsp;Группы пользователей
					</NavLink>
				),
				minimalAccess: AccessType.NotSet,
			},
			{
				key: 'settings',
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
			defaultOpenKeys={['tags']}
		/>
	)
})

export default AppMenu
