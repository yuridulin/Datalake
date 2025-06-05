import { TagType } from '@/api/swagger/data-contracts'
import { TagValue } from '@/types/tagValue'

type TagValueProps = {
	value: TagValue
	type?: TagType
}

const TagValueText = ({ value, type = TagType.String }: TagValueProps) => {
	if (type === TagType.Boolean) return <span style={{ color: '#5273e0' }}>{value ? 'true' : 'false'}</span>
	if (type === TagType.Number) return <span style={{ color: '#e87040' }}>{value ?? '?'}</span>
	if (type === TagType.String) return <span style={{ color: '#e87040' }}>{value ?? '?'}</span>
	return <span>{value ?? '?'}</span>
}

export default TagValueText
