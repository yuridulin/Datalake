import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Button, Col, Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import { user } from '../../state/user'

const LogoPanel = observer(() => {
	return (
		<Row align='middle' className='app-two-items'>
			<Col span={12}>
				<Link to='/' className='title'>
					Datalake
				</Link>
			</Col>
			<Col span={12} style={{ textAlign: 'right' }}>
				<Button
					type='link'
					onClick={() =>
						user.setTheme(user.isDark() ? 'light' : 'dark')
					}
					title={
						'Изменить тему на ' +
						(user.isDark() ? 'светлую' : 'темную')
					}
				>
					{user.isDark() ? <MoonOutlined /> : <SunOutlined />}
				</Button>
			</Col>
		</Row>
	)
})

export default LogoPanel
