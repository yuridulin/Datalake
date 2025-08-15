import { TagQuality, TagType } from '@/api/swagger/data-contracts'
import TagQualityEl from '@/app/components/TagQualityEl'
import { TagValue } from '@/types/tagValue'
import { theme } from 'antd'

type TagCompactOptions = {
	value: TagValue
	type: TagType
	quality: TagQuality | null
}

export default function TagCompactValue({ value, type, quality }: TagCompactOptions) {
	const { token } = theme.useToken()

	let color = ''
	let v: TagValue = ''

	switch (type) {
		case TagType.String:
			color = '#6abe39'
			v = value ?? '?'
			break
		case TagType.Boolean:
			color = '#5273e0'
			v = value ? 'True' : 'False'
			break
		case TagType.Number:
			color = '#e87040'
			v = value ?? '?'
			break
	}

	return (
		<>
			{quality !== null && (
				<>
					{<TagQualityEl quality={quality} />}&ensp;
					<span style={{ color: token.colorBorder }}>|</span>
					&ensp;
				</>
			)}
			<span style={{ color: color }}>{v}</span>
		</>
	)
}
