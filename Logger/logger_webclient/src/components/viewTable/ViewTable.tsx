import React from "react"
import './ViewTable.css'

type ViewTableProps = {
	headers: string[]
	children: React.ReactNode
}

export default function ViewTable({ headers, children }: ViewTableProps) {
	return <div className="table">
		<div className="table-header">
			<div>
				{headers.map(x => <div>{x}</div>)}
			</div>
		</div>
		<div className="table-body">
			{children}
		</div>
	</div>
}