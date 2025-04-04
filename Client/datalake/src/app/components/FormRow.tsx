import { ReactNode } from 'react'

type HeaderProps = {
	title?: ReactNode
	children?: ReactNode
	style?: React.CSSProperties
}

export default function FormRow({ title, children, style }: HeaderProps) {
	return (
		<div style={{ marginBottom: '1em' }}>
			<span style={{ display: 'block', marginBottom: '.25em' }}>{title}</span>
			<div style={style}>{children}</div>
		</div>
	)
}
