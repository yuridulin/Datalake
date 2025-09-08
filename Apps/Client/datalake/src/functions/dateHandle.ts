import dayjs, { Dayjs } from 'dayjs'
import 'dayjs/locale/ru'
import TimeAgo from 'javascript-time-ago'
import ru from 'javascript-time-ago/locale/ru'

// настройка времени
dayjs.locale('ru')
const timeMask = 'YYYY-MM-DDTHH:mm:ss'

TimeAgo.addLocale(ru)
const timeAgo = new TimeAgo('ru-RU')

// Сериализация даты в строку
const serializeDate = (date: Dayjs | string | null | undefined): string => {
	return (date ? (typeof date === 'string' ? deserializeDate(date) : date) : dayjs()).format(timeMask)
}

// Извлечение даты из строки
const deserializeDate = (dateString: string | null, fallback: dayjs.Dayjs = dayjs()): Dayjs => {
	return dateString ? dayjs(dateString, timeMask) : fallback
}

export { deserializeDate, serializeDate, timeAgo }
