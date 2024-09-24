import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { useUpdateContext } from '../../../context/updateContext'
import { useInterval } from '../../../hooks/useInterval'

export default function Offline() {
	const [online, setOnline] = useState(false)

	const { isDarkMode } = useUpdateContext()

	function load() {
		api.configGetLastUpdate()
			.then((res) => !!res && setOnline(!!res.data))
			.catch(() => setOnline(false))
	}

	useInterval(load, 5000)

	return online ? (
		<Navigate to={'/'} />
	) : (
		<div
			style={{
				position: 'fixed',
				top: 0,
				left: 0,
				right: 0,
				bottom: 0,
				backgroundColor: isDarkMode ? '#121212' : '#fff',
			}}
		>
			<div className='offline'>Сервер не отвечает</div>
		</div>
	)
}
