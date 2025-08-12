import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Button, Col, Row, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import { user } from '../../state/user'

declare const VERSION: string

const LogoPanel = observer(() => {
	return (
		<Row align='middle' className='app-two-items'>
			<Col span={16}>
				<Link to='/' className='title'>
					Datalake {VERSION && <Tag>v{VERSION}</Tag>}
				</Link>
			</Col>
			<Col span={8} style={{ textAlign: 'right' }}>
				<Button
					type='link'
					onClick={() => user.setTheme(user.isDark() ? 'light' : 'dark')}
					title={'Изменить тему на ' + (user.isDark() ? 'светлую' : 'темную')}
				>
					{user.isDark() ? <MoonOutlined /> : <SunOutlined />}
				</Button>
			</Col>
		</Row>
	)
})

export default LogoPanel
