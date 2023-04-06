import { Navigate } from 'react-router-dom'
import { useInterval } from '../../hooks/useInterval'
import { useFetching } from '../../hooks/useFetching'
import { useState } from 'react'
import appApi from '../../api/appApi'

export default function Offline() {

	const [ online, setOnline ] = useState(false)

	const [ load ] = useFetching(async () => {
		let res = await appApi.lastUpdate()
		setOnline(!!res)
	})

	useInterval(() => { load() }, 5000)

	return (
		online
			? <Navigate to={'/'} />
			: <div className="offline">Сервер не отвечает</div>
	)
}