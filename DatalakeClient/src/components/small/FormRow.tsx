import { ReactNode } from "react"

type HeaderProps = {
	title?: ReactNode
	children?: ReactNode
	style?: React.CSSProperties
}

export default function FormRow({ title, children, style }: HeaderProps) {
	return <div className="form-row">
		<span>{title}</span>
		<div style={style}>{children}</div>
	</div>
}