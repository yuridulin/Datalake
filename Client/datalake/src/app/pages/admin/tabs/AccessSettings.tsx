import api from '@/api/swagger-api'
import { UserAuthInfo } from '@/api/swagger/data-contracts'
import { user } from '@/state/user'
import { Button, Divider, Space, Spin } from 'antd'
import React, { useEffect, useState } from 'react'
import JsonView from 'react18-json-view'
import 'react18-json-view/src/style.css'

if (user.isDark()) {
	import('react18-json-view/src/dark.css')
}

const AccessSettings = () => {
	const [rights, setRights] = useState({} as { [key: string]: UserAuthInfo })
	const [wait, setWait] = useState(false)

	const restart = () => {
		setWait(true)
		api.systemRestartAccess().then(() => {
			setWait(false)
			load()
		})
	}

	const load = () => {
		api.systemGetAccess().then((res) => setRights(res.data))
	}

	useEffect(load, [])

	return (
		<>
			<Space onClick={restart}>
				<Button disabled={wait}>
					{wait && <Spin />} Перерасчет прав доступа
				</Button>
			</Space>
			<Divider>
				<small>Текущие права доступа</small>
			</Divider>
			<React.Fragment>
				<JsonView src={rights} />
			</React.Fragment>
		</>
	)
}

export default AccessSettings
