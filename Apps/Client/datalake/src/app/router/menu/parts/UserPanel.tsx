import AccessTypeEl from '@/app/components/AccessTypeEl'
import { useAppStore } from '@/store/useAppStore'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'

const UserPanel = observer(() => {
	const store = useAppStore()

	return (
		<table style={{ width: '100%', maxWidth: '100%' }}>
			<tbody>
				<tr>
					<td
						colSpan={2}
						style={{
							padding: '.25em 1em',
							wordBreak: 'break-word',
						}}
					>
						{store.fullName}
					</td>
				</tr>
				<tr>
					<td style={{ padding: '.25em 0 .25em 1em', width: '1em' }}>
						<UserOutlined />
					</td>
					<td style={{ padding: '.25em 1em' }}>
						<AccessTypeEl type={store.globalAccessType} />
					</td>
					<td style={{ padding: '.25em 1em .25em 0', width: '1em' }}>
						<Button type='link' onClick={store.logout} title='Выход из учетной записи'>
							<LogoutOutlined />
						</Button>
					</td>
				</tr>
			</tbody>
		</table>
	)
})

export default UserPanel
