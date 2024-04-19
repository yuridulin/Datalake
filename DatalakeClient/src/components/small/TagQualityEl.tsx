import {
	CheckOutlined,
	DisconnectOutlined,
	EditOutlined,
	WarningOutlined,
} from '@ant-design/icons'
import { TagQuality } from '../../api/data-contracts'

export default function TagQualityEl({ quality }: { quality: TagQuality }) {
	switch (quality) {
		case TagQuality.Bad:
			return <WarningOutlined title='Значение не достоверно' />
		case TagQuality.BadNoConnect:
			return <DisconnectOutlined title='Потеря связи' />
		case TagQuality.BadNoValues:
			return <DisconnectOutlined title='Значения не получены' />
		case TagQuality.BadManualWrite:
			return <WarningOutlined title='Значение не достоверно' />
		case TagQuality.Good:
			return <CheckOutlined title='Достоверное значение' />
		case TagQuality.GoodManualWrite:
			return <EditOutlined title='Достоверное значение, ручной ввод' />
		default:
			return <WarningOutlined title='Значение не достоверно' />
	}
}
