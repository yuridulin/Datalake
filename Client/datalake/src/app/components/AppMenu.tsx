import { Menu, theme } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../api/models/customSource'
import routes from '../router/routes'

const items: ItemType<MenuItemType>[] = [
	{
		key: 'admin',
		label: 'Администрирование',
		type: 'group',
		children: [
			{
				key: 'logs',
				label: <NavLink to={'/'}>Журнал</NavLink>,
			},
			{
				key: 'users',
				label: <NavLink to={routes.Users.List}>Пользователи</NavLink>,
			},
			/* {
				key: 'user-groups',
				label: (
					<NavLink to={routes.UserGroups.List}>
						Группы пользователей
					</NavLink>
				),
			}, */
		],
	},
	{
		key: 'sources',
		label: 'Источники данных',
		type: 'group',
		children: [
			{
				key: 'sources-list',
				label: <NavLink to={'/sources'}>Список источников</NavLink>,
			},
		],
	},
	{
		key: 'blocks',
		label: 'Объекты',
		type: 'group',
		children: [
			{
				key: 'blocks-tree',
				label: <NavLink to={'/blocks'}>Дерево объектов</NavLink>,
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
				label: <NavLink to={'/tags'}>Все теги</NavLink>,
			},
			{
				key: CustomSource.Manual,
				label: <NavLink to={'/tags/manual/'}>Мануальные теги</NavLink>,
			},
			{
				key: CustomSource.Calculated,
				label: <NavLink to={'/tags/calc/'}>Вычисляемые теги</NavLink>,
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
					<NavLink to={routes.Viewer.root + routes.Viewer.TagsViewer}>
						Запросы
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
