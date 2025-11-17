import {
	ApiOutlined,
	BorderOutlined,
	CalculatorOutlined,
	CheckCircleOutlined,
	FormOutlined,
	MoreOutlined,
	QuestionOutlined,
} from '@ant-design/icons'
import { theme } from 'antd'
import { TagQuality } from '../../generated/data-contracts'

type TagQualityElProps = {
	quality?: TagQuality
}

const TagQualityEl = ({ quality }: TagQualityElProps) => {
	const { token } = theme.useToken()

	switch (quality) {
		case TagQuality.Good:
			return <CheckCircleOutlined style={{ color: token.colorSuccess }} title='Достоверное значение' />

		case TagQuality.BadNoConnect:
			return <ApiOutlined style={{ color: token.colorError }} title='Потеря связи' />
		case TagQuality.BadNoValues:
			return <BorderOutlined style={{ color: token.colorError }} title='Значения не существует' />
		case TagQuality.BadCalcError:
			return <CalculatorOutlined style={{ color: token.colorError }} title='Ошибка при вычислении' />

		case TagQuality.GoodManualWrite:
			return <FormOutlined style={{ color: token.colorSuccess }} title='Достоверное значение, ручной ввод' />
		case TagQuality.BadManualWrite:
			return <FormOutlined style={{ color: token.colorError }} title='Значение не достоверно, ручной ввод' />

		case TagQuality.GoodLOCF:
			return <MoreOutlined style={{ color: token.colorSuccess }} title='Достоверное значение, протянуто' />
		case TagQuality.BadLOCF:
			return <MoreOutlined style={{ color: token.colorError }} title='Значение не достоверно, протянуто' />

		default:
			return <QuestionOutlined title={String(quality)} />
	}
}

export default TagQualityEl
