import React from "react"
import './FormItem.css'

type FormItemProps = {
	caption: string
	children: React.ReactNode
}

export default function FormItem({ caption, children }: FormItemProps) {
	return (
		<div className="form-item">
			<span>{caption}</span>
			{children}
		</div>
	)
}