import { ReactNode } from 'react'

type HeaderProps = {
	left?: ReactNode
	right?: ReactNode
	children?: ReactNode
}

export default function Header({ left, children, right }: HeaderProps) {
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
