import { LogCategory } from '../api/swagger/data-contracts'

export default function getLogCategodyName(category: LogCategory) {
	switch (category) {
		case LogCategory.Api:
			return 'API'
		case LogCategory.Calc:
			return 'Вычислитель'
		case LogCategory.Collector:
			return 'Сборщики'
		case LogCategory.Core:
			return 'Ядро'
		case LogCategory.Database:
			return 'База данных'
		case LogCategory.Http:
			return 'HTTP'
		case LogCategory.Source:
			return 'Источники'
		case LogCategory.Tag:
			return 'Теги'
		case LogCategory.UserGroups:
			return 'Группы пользователей'
		case LogCategory.Users:
			return 'Пользователи'
		case LogCategory.Blocks:
			return 'Блоки'
		default:
			return category
	}
}
