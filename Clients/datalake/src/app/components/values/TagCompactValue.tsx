import TagQualityEl from '@/app/components/TagQualityEl'
import { TagQuality, TagType, ValueRecord } from '@/generated/data-contracts'
import { theme } from 'antd'

type TagCompactOptions = {
	record: ValueRecord | null
	type: TagType
	quality: TagQuality | null
}

const TagCompactValue = ({ record, type: tagType, quality }: TagCompactOptions) => {
	const { token } = theme.useToken()

	let color = ''
	let v: string = '?'

	if (record) {
		switch (tagType) {
			case TagType.String:
				color = '#6abe39'
				v = record.text ?? '?'
				break
			case TagType.Boolean:
				color = '#5273e0'
				v = record.boolean != null ? (record.boolean ? 'True' : 'False') : '?'
				break
			case TagType.Number:
				color = '#e87040'
				v = record.number != null ? String(record.number) : '?'
				break
		}
	}

	return (
		<>
			{quality !== null && (
				<>
					<TagQualityEl quality={quality} />
					&ensp;
					<span style={{ color: token.colorBorder }}>|</span>
					&ensp;
				</>
			)}
			<span style={{ color: color }}>{v}</span>
		</>
	)
}

export default TagCompactValue
