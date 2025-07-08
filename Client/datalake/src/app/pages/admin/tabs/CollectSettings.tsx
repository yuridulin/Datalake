import api from '@/api/swagger-api'
import { Button, Space, Spin } from 'antd'
import { useState } from 'react'

const CollectSettings = () => {
	const [wait, setWait] = useState(false)

	const restartState = () => {
		setWait(true)
		api.systemRestartState().then((res) => res.status === 204 && setWait(false))
	}
	const restartValues = () => {
		setWait(true)
		api.systemRestartValues().then((res) => res.status === 204 && setWait(false))
	}

	return (
		<Space>
			<Button onClick={restartState} disabled={wait}>
				{wait && <Spin />} Перезагрузка состояния данных из БД
			</Button>
			<Button onClick={restartValues} disabled={wait}>
				{wait && <Spin />} Перезагрузка текущих данных из БД
			</Button>
		</Space>
	)
}

export default CollectSettings
