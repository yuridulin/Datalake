import { notification } from 'antd'
import appTheme from './theme-settings'

notification.config({
	placement: 'bottomLeft',
	bottom: 50,
	closable: true,
	duration: 5,
})

const notify = {
	done(message: string = 'Успешно') {
		notification.success({
			message: message,
			className: appTheme.isDark() ? 'notify-dark' : '',
		})
	},
	err(message: string) {
		notification.error({
			message: message,
			className: appTheme.isDark() ? 'notify-dark' : '',
		})
	},
}

export default notify
