import AccessTypeEl from '@/app/components/AccessTypeEl'
import { UserType } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { LogoutOutlined, MoonOutlined, SunOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
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
					{store.fullName}
				</div>
				<div style={right}>
					<Button type='link' onClick={store.logout} title='Выход из учетной записи'>
						<LogoutOutlined />
					</Button>
				</div>
			</div>
			<div style={row}>
				<div style={left}>
					<AccessTypeEl bordered={false} type={store.globalAccessType} />
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
