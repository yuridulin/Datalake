import { CheckOutlined, DisconnectOutlined, EditOutlined, WarningOutlined } from "@ant-design/icons";
import { ReactElement } from "react";

export default function TagQuality({ quality }: { quality: number }) {
	
	const qualityEnum: { [key: number]: ReactElement } = {
		0: <WarningOutlined title="Значение не достоверно" />,
		4: <DisconnectOutlined title="Потеря связи" />,
		192: <CheckOutlined title="Достоверное значение" />,
		216: <EditOutlined title="Достоверное значение, ручной ввод" />
	}

	return (
		qualityEnum[quality] ?? qualityEnum[0]
	)
}