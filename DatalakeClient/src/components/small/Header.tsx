import { ReactNode } from 'react'

type HeaderProps = {
	left?: ReactNode
	right?: ReactNode
	children?: ReactNode
}

export default function Header({ left, children, right }: HeaderProps) {
	return (
		<div className='header'>
			{left && <div className='header-left'>{left}</div>}
			{children && <div className='header-center'>{children}</div>}
			{right && <div className='header-right'>{right}</div>}
		</div>
	)
}
