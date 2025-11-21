import AccessTypeEl from '@/app/components/AccessTypeEl'
import { UserType } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { LogoutOutlined, MoonOutlined, SunOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'

const panel: React.CSSProperties = {
	display: 'flex',
	flexDirection: 'column',
}

const row: React.CSSProperties = {
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center',
	paddingBottom: '0.25em',
}

const left: React.CSSProperties = {
	paddingLeft: '1em',
	wordBreak: 'break-word',
}

const right: React.CSSProperties = {
	paddingRight: '0.5em',
}

const title: React.CSSProperties = {
	marginBottom: '1em',
	fontWeight: 500,
	fontSize: 'large',
	color: '#4096ff',
	textDecoration: 'none',
}

const UserPanel = observer(() => {
	const store = useAppStore()

	const [name, setName] = useState('')
	const hasLoadedRef = useRef(false)
	const lastUserGuidRef = useRef<string | null>(store.userGuid)

	useEffect(() => {
		// Если изменился userGuid, сбрасываем флаг загрузки
		if (lastUserGuidRef.current !== store.userGuid) {
			hasLoadedRef.current = false
			lastUserGuidRef.current = store.userGuid
		}

		if (hasLoadedRef.current) return
		hasLoadedRef.current = true

		if (store.userGuid === null) {
			setName('')
		} else {
			store.api.inventoryUsersGet({ userGuid: store.userGuid }).then((res) => {
				setName(res.data[0].fullName)
			})
		}
	}, [store.api, store.userGuid])

	return (
		<div style={panel}>
			<div style={row}>
				<div style={left}>
					<Link to='/' style={title}>
						Datalake&ensp;
						<Tag bordered={false}>{store.version}</Tag>
					</Link>
				</div>
				<div style={right}>
					<Button
						type='link'
						onClick={store.switchTheme}
						title={'Изменить тему на ' + (store.isDark ? 'светлую' : 'темную')}
					>
						{store.isDark ? <MoonOutlined /> : <SunOutlined />}
					</Button>
				</div>
			</div>
			<div style={row}>
				<div style={left}>
					<UserOutlined />
					&ensp;
					{name}
				</div>
				<div style={right}>
					<Button type='link' onClick={store.logout} title='Выход из учетной записи'>
						<LogoutOutlined />
					</Button>
				</div>
			</div>
			<div style={row}>
				<div style={left}>
					<AccessTypeEl bordered={false} type={store.rootRule.access} />
				</div>
				<div style={right}>
					{store.type === UserType.Local ? (
						<Tag bordered={false}>LOCAL</Tag>
					) : store.type === UserType.EnergoId ? (
						<Tag bordered={false}>EnergoID</Tag>
					) : (
						<></>
					)}
				</div>
			</div>
		</div>
	)
})

export default UserPanel
