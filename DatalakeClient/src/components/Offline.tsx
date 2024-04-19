import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { Api } from '../api/Api'
import { useFetching } from '../hooks/useFetching'
import { useInterval } from '../hooks/useInterval'

export default function Offline() {
	const [online, setOnline] = useState(false)
	const api = new Api()

	const [load] = useFetching(async () => {
		await api.configLast().then((res) => setOnline(!!res.data))
	})

	useInterval(() => {
		load()
	}, 5000)

	return online ? (
		<Navigate to={'/'} />
	) : (
		<div className='offline'>Сервер не отвечает</div>
	)
}
