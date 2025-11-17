import { SourceType } from '@/generated/data-contracts'
import { blue } from '@ant-design/colors'
import {
	CalculatorOutlined,
	EditOutlined,
	LineChartOutlined,
	QuestionCircleOutlined,
	TagOutlined,
} from '@ant-design/icons'

type TagIconProps = {
	type: SourceType
}

const TagIcon = ({ type }: TagIconProps) => {
	switch (type) {
		case SourceType.Manual:
			return <EditOutlined style={{ color: blue[4] }} />
		case SourceType.Calculated:
			return <CalculatorOutlined style={{ color: blue[4] }} />
		case SourceType.System:
			return <LineChartOutlined style={{ color: blue[4] }} />
		case SourceType.Unset:
			return <QuestionCircleOutlined style={{ color: blue[4] }} />
		default:
			return <TagOutlined style={{ color: blue[4] }} />
	}
}

export default TagIcon
