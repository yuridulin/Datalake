import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import api from '../api/swagger-api'
import { useInterval } from '../hooks/useInterval'

export default function Offline() {
	const [online, setOnline] = useState(false)

	function load() {
		api.configGetLastUpdate().then((res) => !!res && setOnline(!!res.data))
	}

	useInterval(() => {
		load()
	}, 5000)

	return online ? (
		<Navigate to={'/'} />
	) : (
		<div className='offline'>Сервер не отвечает</div>
	)
}
