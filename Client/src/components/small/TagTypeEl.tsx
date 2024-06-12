import { Tag } from 'antd'
import { TagType } from '../../api/swagger/data-contracts'

export default function TagTypeEl({ tagType }: { tagType: TagType }) {
	if (tagType === TagType.String) return <Tag color='green'>строка</Tag>

	if (tagType === TagType.Number) return <Tag color='volcano'>число</Tag>

	if (tagType === TagType.Boolean) return <Tag color='geekblue'>дискрет</Tag>

	return <Tag>?</Tag>
}
