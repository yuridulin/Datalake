import TimeAgo from 'javascript-time-ago'
import ru from 'javascript-time-ago/locale/ru'

TimeAgo.addLocale(ru)

const timeAgo = new TimeAgo('ru-RU')

export { timeAgo }
