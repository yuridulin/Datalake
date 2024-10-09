/* eslint-disable react-refresh/only-export-components */
import { MoonOutlined, SunOutlined } from '@ant-design/icons'
import { Button, Col, Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import { useUpdateContext } from '../../context/updateContext'

const LogoPanel = () => {
	const { isDarkMode, setDarkMode } = useUpdateContext()

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
					onClick={() => setDarkMode(!isDarkMode)}
					title={
						'Изменить тему на ' +
						(isDarkMode ? 'светлую' : 'темную')
					}
				>
					{isDarkMode ? <MoonOutlined /> : <SunOutlined />}
				</Button>
			</Col>
		</Row>
	)
}

export default observer(LogoPanel)
