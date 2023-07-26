import { Navigate } from 'react-router-dom'
import { useInterval } from '../hooks/useInterval'
import { useFetching } from '../hooks/useFetching'
import { useState } from 'react'
import axios from 'axios'

export default function Offline() {

	const [ online, setOnline ] = useState(false)

	const [ load ] = useFetching(async () => {
		let res = await axios.post('/config/lastUpdate')
		setOnline(!!res.data)
	})

	useInterval(() => { load() }, 5000)

	return (
		online
			? <Navigate to={'/'} />
			: <div className="offline">Сервер не отвечает</div>
	)
}