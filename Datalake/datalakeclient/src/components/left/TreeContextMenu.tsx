import { MenuProps } from "antd"
import { API } from "../../router/api"

export const items: MenuProps['items'] = [
	{
		label: 'Добавить новый источник данных',
		key: API.sources.create,
	},
	{
		label: 'Добавить новый объект',
		key: API.blocks.create,
	},
	{
		label: 'Добавить новый тег',
		key: API.tags.create,
	},
]