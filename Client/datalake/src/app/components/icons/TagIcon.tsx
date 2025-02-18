import { SourceType } from '@/api/swagger/data-contracts'
import { blue } from '@ant-design/colors'
import {
	CalculatorOutlined,
	EditOutlined,
	LineChartOutlined,
	QuestionCircleOutlined,
	TagOutlined,
} from '@ant-design/icons'

const TagIcon = ({ type }: { type: SourceType }) => {
	switch (type) {
		case SourceType.Manual:
			return <EditOutlined style={{ color: blue[4] }} />
		case SourceType.Calculated:
			return <CalculatorOutlined style={{ color: blue[4] }} />
		case SourceType.System:
			return <LineChartOutlined style={{ color: blue[4] }} />
		case SourceType.NotSet:
			return <QuestionCircleOutlined style={{ color: blue[4] }} />
		default:
			return <TagOutlined style={{ color: blue[4] }} />
	}
}

export default TagIcon
