import { Tag } from 'antd'
import { LogType } from '../../api/swagger/data-contracts'

const colors = {
	[LogType.Error]: 'volcano',
	[LogType.Warning]: 'geekblue',
	[LogType.Success]: 'green',
	[LogType.Information]: 'geekblue',
	[LogType.Trace]: 'inherit',
}

export default function LogTypeEl({ type }: { type: LogType }) {
	return <Tag color={colors[type]}>{LogType[type]}</Tag>
}
