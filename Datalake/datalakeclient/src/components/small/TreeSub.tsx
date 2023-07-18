import { useEffect, useState } from "react"
import TreeItem from "./TreeItem"

export type TreeSubProps = {
	id: string
	icon: string
	to: string
	text: string
	elements?: TreeSubProps[]
}

export default function TreeSub({ id, icon, to, text, elements = [] }: TreeSubProps) {

	const [ open, setOpen ] = useState(false)

	function toggle() {
		if (open) {
			setOpen(false)
			localStorage.setItem(id, 'close')
		} else {
			setOpen(true)
			localStorage.setItem(id, 'open')
		}
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { setOpen(localStorage.getItem(id) === 'open') }, [])

	return (
		elements.length === 0
		? <TreeItem to={to} icon={icon} text={text} />
		: <div className="tree-block">
			<TreeItem to={to} icon={icon} text={text} open={open} toggle={toggle} />
			<div className={'tree-sub' + (open ? ' tree-sub-opened' : '')}>
				{elements.map(x => <TreeSub key={x.id} {...x} />)}
			</div>
		</div>
	)
}