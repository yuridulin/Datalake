import { Tag } from "antd";
import { TagType } from "../../@types/enums/TagType";

export default function TagTypeEl({ tagType }: { tagType: TagType }) {
	
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