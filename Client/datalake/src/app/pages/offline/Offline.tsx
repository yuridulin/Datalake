import api from '@/api/swagger-api'
import { CompressOutlined } from '@ant-design/icons'
import { Spin, theme } from 'antd'
import { observer } from 'mobx-react-lite'
import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useInterval } from '../../../hooks/useInterval'

const Offline = observer(() => {
	const [online, setOnline] = useState(false)
	const { token } = theme.useToken()

	function load() {
		api.systemGetLastUpdate()
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
				padding: '2em',
				backgroundColor: token.colorBgContainer,
				color: token.colorText,
			}}
		>
			<Spin indicator={<CompressOutlined spin />} />
			&emsp; Сервер не отвечает... &emsp;
		</div>
	)
})

export default Offline
