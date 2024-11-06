import {
	ApiOutlined,
	BorderOutlined,
	CheckCircleOutlined,
	CloseCircleOutlined,
	FormOutlined,
	MoreOutlined,
	QuestionOutlined,
} from '@ant-design/icons'
import { theme } from 'antd'
import { TagQuality } from '../../api/swagger/data-contracts'

type TagQualityElProps = {
	quality?: TagQuality
}

const TagQualityEl = ({ quality }: TagQualityElProps) => {
	const { token } = theme.useToken()

	switch (quality) {
		case TagQuality.Good:
			return (
				<CheckCircleOutlined
					style={{ color: token.colorSuccess }}
					title='Достоверное значение'
				/>
			)

		case TagQuality.Bad:
			return (
				<CloseCircleOutlined
					style={{ color: token.colorError }}
					title='Значение не достоверно'
				/>
			)

		case TagQuality.BadNoConnect:
			return (
				<ApiOutlined
					style={{ color: token.colorError }}
					title='Потеря связи'
				/>
			)
		case TagQuality.BadNoValues:
			return (
				<BorderOutlined
					style={{ color: token.colorError }}
					title='Значения не существует'
				/>
			)

		case TagQuality.GoodManualWrite:
			return (
				<FormOutlined
					style={{ color: token.colorSuccess }}
					title='Достоверное значение, ручной ввод'
				/>
			)
		case TagQuality.BadManualWrite:
			return (
				<FormOutlined
					style={{ color: token.colorError }}
					title='Значение не достоверно, ручной ввод'
				/>
			)

		case TagQuality.GoodLOCF:
			return (
				<MoreOutlined
					style={{ color: token.colorSuccess }}
					title='Достоверное значение, протянуто'
				/>
			)
		case TagQuality.BadLOCF:
			return (
				<MoreOutlined
					style={{ color: token.colorError }}
					title='Значение не достоверно, протянуто'
				/>
			)

		default:
			return <QuestionOutlined />
	}
}

export default TagQualityEl
