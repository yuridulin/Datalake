import { CheckOutlined, WarningOutlined } from '@ant-design/icons'
import { TagValue } from '../../api/models/tagValue'
import { TagQuality, TagType } from '../../api/swagger/data-contracts'

type TagCompactOptions = {
	value: TagValue
	type: TagType
	quality: TagQuality
}

export default function TagCompactValue({
	value,
	type,
	quality,
}: TagCompactOptions) {
	let color = ''
	let good = true
	let v: TagValue = ''

	switch (type) {
		case TagType.String:
			color = '#6abe39'
			v = value ?? '?'
			break
		case TagType.Boolean:
			color = '#5273e0'
			v = value == 'true' ? 'True' : 'False'
			break
		case TagType.Number:
			color = '#e87040'
			v = value ?? '?'
			break
	}

	switch (quality) {
		case TagQuality.Bad:
			good = false
			break
		case TagQuality.BadManualWrite:
			good = false
			break
		case TagQuality.BadNoConnect:
			good = false
			break
		case TagQuality.BadNoValues:
			good = false
			break
	}

	return (
		<>
			<span style={{ color: color }}>
				{v}
				&ensp;
				{good ? <CheckOutlined /> : <WarningOutlined />}
			</span>
		</>
	)
}
