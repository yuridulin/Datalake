import { Tag } from "antd";

export default function TagType({ tagType }: { tagType: number }) {
	
	if (tagType === 0) return (
		<Tag color="green">строка</Tag>
	)

	if (tagType === 1) return (
		<Tag color="volcano">число</Tag>
	)

	if (tagType === 2) return (
		<Tag color="geekblue">дискрет</Tag>
	)

	return (
		<Tag>?</Tag>
	)
}