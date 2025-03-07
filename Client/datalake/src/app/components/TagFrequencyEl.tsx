import getTagFrequencyName from '@/functions/getTagFrequencyName'
import { blue } from '@ant-design/colors'
import { TagFrequency } from '../../api/swagger/data-contracts'

type TagFrequencyElProps = {
	frequency: TagFrequency
}

const TagFrequencyEl = ({ frequency }: TagFrequencyElProps) => {
	const name = getTagFrequencyName(frequency)
	const color = blue[4]
	const fontWeight = '500'

	switch (frequency) {
		case TagFrequency.ByDay:
			return (
				<b title={name} style={{ color, fontWeight }}>
					D
				</b>
			)
		case TagFrequency.ByHour:
			return (
				<b title={name} style={{ color, fontWeight }}>
					H
				</b>
			)
		case TagFrequency.ByMinute:
			return (
				<b title={name} style={{ color, fontWeight }}>
					M
				</b>
			)
		default:
			return <></>
	}
}

export default TagFrequencyEl
