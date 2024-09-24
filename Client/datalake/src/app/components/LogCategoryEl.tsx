import { Tag } from 'antd'
import { LogCategory } from '../../api/swagger/data-contracts'

type LogCategoryProps = {
	category: LogCategory
}

export default function LogCategoryEl({ category }: LogCategoryProps) {
	switch (category) {
		case LogCategory.Api:
			return <Tag>API</Tag>
		case LogCategory.Calc:
			return <Tag>Вычислитель</Tag>
		case LogCategory.Collector:
			return <Tag>Сборщики</Tag>
		case LogCategory.Core:
			return <Tag>Ядро</Tag>
		case LogCategory.Database:
			return <Tag>База данных</Tag>
		case LogCategory.Http:
			return <Tag>HTTP</Tag>
		case LogCategory.Source:
			return <Tag>Источники</Tag>
		case LogCategory.Tag:
			return <Tag>Теги</Tag>
		case LogCategory.UserGroups:
			return <Tag>Группы пользователей</Tag>
		case LogCategory.Users:
			return <Tag>Пользователи</Tag>
		default:
			return <Tag>{category}</Tag>
	}
}
