import { LogType } from '../api/swagger/data-contracts'

export default function getLogTypeName(type: LogType) {
	switch (type) {
		case LogType.Error:
			return 'Ошибка'
		case LogType.Information:
			return 'Информация'
		case LogType.Success:
			return 'Успех'
		case LogType.Warning:
			return 'Предупреждение'
		default:
			return 'Сообщение'
	}
}
