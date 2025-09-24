import { useAppStore } from '@/store/useAppStore'
import { Button, Space, Spin } from 'antd'
import { useState } from 'react'

const CollectSettings = () => {
	const store = useAppStore()
	const [wait, setWait] = useState(false)

	const restartState = () => {
		setWait(true)
		store.api.systemRestartState().then((res) => res.status === 204 && setWait(false))
	}
	const restartValues = () => {
		setWait(true)
		store.api.systemRestartValues().then((res) => res.status === 204 && setWait(false))
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
