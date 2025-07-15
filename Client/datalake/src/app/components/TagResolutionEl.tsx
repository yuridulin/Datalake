import getTagResolutionName from '@/functions/getTagResolutionName'
import { blue } from '@ant-design/colors'
import { TagResolution } from '../../api/swagger/data-contracts'

type TagResolutionElProps = {
	resolution: TagResolution
	full?: boolean
}

const TagResolutionEl = ({ resolution, full = false }: TagResolutionElProps) => {
	const name = getTagResolutionName(resolution)
	const color = blue[4]
	const fontWeight = '500'

	switch (resolution) {
		case TagResolution.ByMonth:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Посуточный' : 'месяц'}
				</b>
			)
		case TagResolution.ByWeek:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Посуточный' : 'неделя'}
				</b>
			)
		case TagResolution.ByDay:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Посуточный' : 'сутки'}
				</b>
			)
		case TagResolution.ByHalfHour:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Почасовой' : 'получас'}
				</b>
			)
		case TagResolution.ByHour:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Почасовой' : 'час'}
				</b>
			)
		case TagResolution.ByMinute:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Поминутный' : 'минута'}
				</b>
			)
		case TagResolution.BySecond:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Поминутный' : 'секунда'}
				</b>
			)
		default:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'По изменению' : ''}
				</b>
			)
	}
}

export default TagResolutionEl
