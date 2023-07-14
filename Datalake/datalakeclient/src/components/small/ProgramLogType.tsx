import { Tag } from "antd";

export default function ProgramLogType({ type }: { type: number }) {
	
	if (type === 0) return (
		<Tag color="green">отладка</Tag>
	)

	if (type === 1) return (
		<Tag color="volcano">информация</Tag>
	)

	if (type === 2) return (
		<Tag color="geekblue">успех</Tag>
	)

	if (type === 3) return (
		<Tag color="geekblue">предупреждение</Tag>
	)

	if (type === 4) return (
		<Tag color="geekblue">ошибка</Tag>
	)

	return (
		<Tag>?</Tag>
	)
}