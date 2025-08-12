import getTagResolutionName from '@/functions/getTagResolutionName'
import { blue } from '@ant-design/colors'
import { TagResolution } from '../../api/swagger/data-contracts'

type TagResolutionElProps = {
	resolution: TagResolution
	full?: boolean
}

const style = {
	color: blue[4],
	fontWeight: '500',
}

const TagResolutionEl = ({ resolution, full = false }: TagResolutionElProps) => {
	const name = getTagResolutionName(resolution)
	const text = getTagResolutionName(resolution, full)

	return (
		<b title={name} style={style}>
			{text}
		</b>
	)
}

export default TagResolutionEl
