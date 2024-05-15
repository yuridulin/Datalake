import { Tag } from 'antd'
import { LogType } from '../../api/swagger/data-contracts'

const colors = {} as any
colors[LogType.Error] = 'volcano'
colors[LogType.Warning] = 'geekblue'
colors[LogType.Success] = 'green'
colors[LogType.Information] = 'geekblue'
colors[LogType.Trace] = 'inherit'

export default function LogTypeEl({ type }: { type: LogType }) {
	return <Tag color={colors[type]}>{LogType[type]}</Tag>
}
