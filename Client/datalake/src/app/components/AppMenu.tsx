import { blue } from '@ant-design/colors'
import {
	CalculatorOutlined,
	EditOutlined,
	PlaySquareOutlined,
	SettingOutlined,
	TeamOutlined,
	UnorderedListOutlined,
	UserOutlined,
} from '@ant-design/icons'
import { Menu, theme } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../api/types/customSource'
import BlockIcon from '../icons/BlockIcon'
import SourceIcon from '../icons/SourceIcon'
import TagIcon from '../icons/TagIcon'
import routes from '../router/routes'

const items: ItemType<MenuItemType>[] = [
	{
		key: 'sources',
		label: 'Источники данных',
		type: 'group',
		children: [
			{
				key: 'sources-list',
				label: (
					<NavLink to={routes.sources.list}>
						<SourceIcon />
						&emsp;Список источников
					</NavLink>
				),
			},
		],
	},
	{
		key: 'blocks',
		label: 'Блоки',
		type: 'group',
		children: [
			{
				key: 'blocks-tree',
				label: (
					<NavLink to={routes.blocks.list}>
						<BlockIcon />
						&emsp;Дерево блоков
					</NavLink>
				),
			},
		],
	},
	{
		key: 'tags-group',
		type: 'group',
		label: 'Теги',
		children: [
			{
				key: 'tags',
				label: (
					<NavLink to={routes.tags.list}>
						<TagIcon />
						&emsp;Все теги
					</NavLink>
				),
			},
			{
				key: CustomSource.Manual,
				label: (
					<NavLink to={routes.tags.manual}>
						<EditOutlined style={{ color: blue[4] }} />
						&emsp;Мануальные теги
					</NavLink>
				),
			},
			{
				key: CustomSource.Calculated,
				label: (
					<NavLink to={routes.tags.calc}>
						<CalculatorOutlined style={{ color: blue[4] }} />
						&emsp;Вычисляемые теги
					</NavLink>
				),
			},
		],
	},
	{
		key: 'viewer',
		label: 'Просмотр данных',
		type: 'group',
		children: [
			{
				key: 'viewer-tags',
				label: (
					<NavLink to={routes.viewer.tagsViewer}>
						<PlaySquareOutlined style={{ color: blue[4] }} />
						&emsp;Запросы
					</NavLink>
				),
			},
		],
	},
	{
		key: 'admin',
		label: 'Администрирование',
		type: 'group',
		children: [
			{
				key: 'logs',
				label: (
					<NavLink to={'/'}>
						<UnorderedListOutlined style={{ color: blue[4] }} />
						&emsp;Журнал
					</NavLink>
				),
			},
			{
				key: 'users',
				label: (
					<NavLink to={routes.users.list}>
						<UserOutlined style={{ color: blue[5] }} />
						&emsp;Пользователи
					</NavLink>
				),
			},
			{
				key: 'user-groups',
				label: (
					<NavLink to={routes.userGroups.list}>
						<TeamOutlined style={{ color: blue[5] }} />
						&emsp;Группы пользователей
					</NavLink>
				),
			},
			{
				key: 'settings',
				label: (
					<NavLink to={routes.settings}>
						<SettingOutlined style={{ color: blue[5] }} />
						&emsp;Настройки
					</NavLink>
				),
			},
		],
	},
]

export function AppMenu() {
	const { token } = theme.useToken()

	return (
		<Menu
			style={{ border: 0, backgroundColor: token.colorBgLayout }}
			items={items}
			mode='inline'
			defaultOpenKeys={['tags']}
		/>
	)
}
