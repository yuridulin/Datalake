import routes from '@/app/router/routes'
import { ArrowLeftOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { ReactNode } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'

type HeaderProps = {
	left?: ReactNode
	right?: ReactNode
	children?: ReactNode
}

export default function PageHeader({ left, children, right }: HeaderProps) {
	const navigate = useNavigate()

	const back = (e: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
		e.preventDefault()
		navigate(-1)
	}

	return (
		<div style={{ display: 'table', width: '100%', marginBottom: '2em' }}>
			{children && (
				<div
					style={{
						display: 'table-cell',
						textAlign: 'left',
						fontWeight: 'bolder',
					}}
				>
					{left && (
						<div
							style={{
								marginRight: '2em',
								display: 'inline-block',
							}}
						>
							<NavLink to={routes.globalRoot} onClick={back}>
								<Button
									icon={<ArrowLeftOutlined />}
									title='Вернуться на предыдущую страницу'
								>
									Назад
								</Button>
							</NavLink>
							&ensp;
							{left}
						</div>
					)}
					{children}
				</div>
			)}
			{right && (
				<div style={{ display: 'table-cell', textAlign: 'right' }}>
					{right}
				</div>
			)}
		</div>
	)
}
