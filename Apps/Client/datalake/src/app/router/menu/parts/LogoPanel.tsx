import { useAppStore } from '@/store/useAppStore'
import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Button, Col, Row, Tag } from 'antd'
import { observer } from 'mobx-react-lite'
import { Link } from 'react-router-dom'

declare const VERSION: string

const LogoPanel = observer(() => {
	const store = useAppStore()

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
					onClick={store.switchTheme}
					title={'Изменить тему на ' + (store.isDark ? 'светлую' : 'темную')}
				>
					{store.isDark ? <MoonOutlined /> : <SunOutlined />}
				</Button>
			</Col>
		</Row>
	)
})

export default LogoPanel
