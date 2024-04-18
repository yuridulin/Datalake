import { CheckOutlined, DisconnectOutlined, EditOutlined, WarningOutlined } from "@ant-design/icons"
import { TagQuality } from "../../@types/enums/TagQuality"

export default function TagQualityEl({ quality }: { quality: TagQuality }) {
	
	switch (quality) {
		case TagQuality.Bad: return <WarningOutlined title="Значение не достоверно" />
		case TagQuality.Bad_NoConnect: return <DisconnectOutlined title="Потеря связи" />
		case TagQuality.Bad_NoValues: return <DisconnectOutlined title="Значения не получены" />
		case TagQuality.Bad_ManualWrite: return <WarningOutlined title="Значение не достоверно" />
		case TagQuality.Good: return <CheckOutlined title="Достоверное значение" />
		case TagQuality.Good_ManualWrite: return <EditOutlined title="Достоверное значение, ручной ввод" />
		default: return <WarningOutlined title="Значение не достоверно" />
	}
}