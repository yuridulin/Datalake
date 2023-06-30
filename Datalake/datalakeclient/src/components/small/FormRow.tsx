import { ReactNode } from "react"

type HeaderProps = {
	title?: ReactNode
	children?: ReactNode
}

export default function FormRow({ title, children }: HeaderProps) {
	return <div className="form-row">
		<span>{title}</span>
		<div>{children}</div>
	</div>
}