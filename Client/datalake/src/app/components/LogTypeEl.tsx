import { Tag } from 'antd'
import getLogTypeName from '../../api/functions/getLogTypeName'
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
	return <Tag color={colors[type]}>{getLogTypeName(type)}</Tag>
}
