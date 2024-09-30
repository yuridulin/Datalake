import { Tag } from 'antd'
import { LogType } from '../../api/swagger/data-contracts'

type LogTypeProps = {
	type: LogType
}

const colors = {
	[LogType.Error]: 'volcano',
	[LogType.Warning]: 'geekblue',
	[LogType.Success]: 'green',
	[LogType.Information]: 'geekblue',
	[LogType.Trace]: 'inherit',
}

export default function LogTypeEl({ type }: LogTypeProps) {
	switch (type) {
		case LogType.Error:
			return <Tag color={colors[type]}>Ошибка</Tag>
		case LogType.Information:
			return <Tag color={colors[type]}>Информация</Tag>
		case LogType.Success:
			return <Tag color={colors[type]}>Успех</Tag>
		case LogType.Warning:
			return <Tag color={colors[type]}>Предупреждение</Tag>
		default:
			return <Tag color={colors[type]}>Сообщение</Tag>
	}
}
