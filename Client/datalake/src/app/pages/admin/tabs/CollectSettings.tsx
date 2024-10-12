import { Button, Space, Spin } from 'antd'
import { useState } from 'react'
import api from '../../../../api/swagger-api'

export default function CollectSettings() {
	const [wait, setWait] = useState(false)
	const restart = () => {
		setWait(true)
		api.configRestart().then((res) => res.status === 204 && setWait(false))
	}

	return (
		<Space onClick={restart}>
			<Button disabled={wait}>
				{wait && <Spin />} Перезапуск всех опросов
			</Button>
		</Space>
	)
}
