import getTagFrequencyName from '@/functions/getTagFrequencyName'
import { blue } from '@ant-design/colors'
import { TagFrequency } from '../../api/swagger/data-contracts'

type TagFrequencyElProps = {
	frequency: TagFrequency
	full?: boolean
}

const TagFrequencyEl = ({ frequency, full = false }: TagFrequencyElProps) => {
	const name = getTagFrequencyName(frequency)
	const color = blue[4]
	const fontWeight = '500'

	switch (frequency) {
		case TagFrequency.ByDay:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Посуточный' : 'day'}
				</b>
			)
		case TagFrequency.ByHour:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Почасовой' : 'hour'}
				</b>
			)
		case TagFrequency.ByMinute:
			return (
				<b title={name} style={{ color, fontWeight }}>
					{full ? 'Поминутный' : 'min'}
				</b>
			)
		default:
			return <></>
	}
}

export default TagFrequencyEl
