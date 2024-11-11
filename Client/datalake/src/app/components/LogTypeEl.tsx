import { Tag } from 'antd'
import { LogType } from '../../api/swagger/data-contracts'
import getLogTypeName from '../../functions/getLogTypeName'

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
