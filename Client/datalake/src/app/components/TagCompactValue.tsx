import { WarningOutlined } from '@ant-design/icons'
import { theme } from 'antd'
import { TagQuality, TagType } from '../../api/swagger/data-contracts'
import { TagValue } from '../../api/types/tagValue'

type TagCompactOptions = {
	value: TagValue
	type: TagType
	quality?: TagQuality
}

export default function TagCompactValue({
	value,
	type,
	quality,
}: TagCompactOptions) {
	const { token } = theme.useToken()

	let color = ''
	const good =
		quality === TagQuality.Good || quality === TagQuality.GoodManualWrite
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

	return (
		<>
			<span style={{ color: color }}>
				{!good && (
					<>
						<WarningOutlined
							title='Значение не достоверно'
							style={{ color: token.colorTextDescription }}
						/>
						&ensp;
					</>
				)}
				{v}
			</span>
		</>
	)
}
