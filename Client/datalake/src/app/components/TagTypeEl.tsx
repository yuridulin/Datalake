import { TagTypeName } from '@/functions/getTagTypeName'
import { Tag } from 'antd'
import { TagType } from '../../api/swagger/data-contracts'

export default function TagTypeEl({ tagType }: { tagType: TagType }) {
	if (tagType === TagType.String) return <Tag color='green'>{TagTypeName[tagType]}</Tag>

	if (tagType === TagType.Number) return <Tag color='volcano'>{TagTypeName[tagType]}</Tag>

	if (tagType === TagType.Boolean) return <Tag color='geekblue'>{TagTypeName[tagType]}</Tag>

	return <Tag>?</Tag>
}
