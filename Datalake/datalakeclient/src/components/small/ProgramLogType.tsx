import { Tag } from "antd";
import { LogType } from "../../@types/enums/LogType";

const colors = {
	0: 'green',
	1: 'volcano',
	2: 'geekblue',
	3: 'geekblue',
	4: 'inherit',
}

export default function ProgramLogType({ type }: { type: keyof typeof LogType }) {

	return (
		<Tag color={colors[type]}>{LogType[type]}</Tag>
	)
}