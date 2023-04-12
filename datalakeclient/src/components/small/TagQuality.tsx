import { CheckOutlined, DisconnectOutlined, EditOutlined, WarningOutlined } from "@ant-design/icons";
import { ReactElement } from "react";

export default function TagQuality({ quality }: { quality: number }) {
	
	const qualityEnum: { [key: number]: ReactElement } = {
		0: <WarningOutlined />,
		4: <DisconnectOutlined />,
		192: <CheckOutlined />,
		216: <EditOutlined />
	}

	return (
		qualityEnum[quality] ?? <WarningOutlined />
	)
}