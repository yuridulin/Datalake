import routes from '@/app/router/routes'
import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button, Space, Typography } from 'antd'
import { ReactNode } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'

const { Text } = Typography

type HeaderProps = {
	left?: ReactNode[]
	right?: ReactNode[]
	icon?: ReactNode
	children: string | string[] | ReactNode
}

const headerCSS: React.CSSProperties = {
	display: 'flex',
	justifyContent: 'space-between',
	alignItems: 'center',
	marginBottom: '2em',
	flexWrap: 'wrap',
	gap: '0.5em',
	height: '2em',
}

const leftCSS: React.CSSProperties = {
	display: 'flex',
	alignItems: 'center',
	gap: '0.5em',
}

const rightCSS: React.CSSProperties = {
	display: 'flex',
	alignItems: 'center',
	gap: '0.5em',
}

const titleCSS: React.CSSProperties = {
	margin: 0,
	fontWeight: 'bold',
	display: 'inline-block',
	textAlign: 'left',
}

export default function PageHeader({ left, icon, children, right }: HeaderProps) {
	const navigate = useNavigate()

	const back = (e: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		e.preventDefault()
		navigate(-1)
	}

	return (
		<div style={headerCSS}>
			<div style={leftCSS}>
				{left && (
					<Space>
						<NavLink to={routes.globalRoot} onClick={back}>
							<Button icon={<ArrowLeftOutlined />} title='Вернуться на предыдущую страницу'>
								Назад
							</Button>
						</NavLink>
					</Space>
				)}
				{left && <>{...left.filter((x) => x)}</>}
				<div style={titleCSS}>
					&emsp;
					{icon && <span style={{ paddingRight: '1em' }}>{icon}</span>}
					<Text>{children}</Text>
				</div>
			</div>

			{right && <div style={rightCSS}>{...right.filter((x) => x)}</div>}
		</div>
	)
}
