import { useEffect, useState } from "react"
import TreeLink from "./TreeLink"
import { TreeItem } from "../../@types/TreeItem"

export type TreeSubProps = {
	item: TreeItem
}

export default function TreeSub({ item }: TreeSubProps) {

	const [ open, setOpen ] = useState(false)

	function toggle() {
		if (open) {
			setOpen(false)
			localStorage.setItem(String(item.Id), 'close')
		} else {
			setOpen(true)
			localStorage.setItem(String(item.Id), 'open')
		}
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { setOpen(localStorage.getItem(String(item.Id)) === 'open') }, [])

	return (
		item.Items.length === 0
		? <TreeLink item={item} />
		: <div className="tree-block">
			<TreeLink item={item} arrow={true} open={open} toggle={toggle} />
			<div className={'tree-sub' + (open ? ' tree-sub-opened' : '')}>
				{item.Items.map((x, i) => <TreeSub key={i} item={x} />)}
			</div>
		</div>
	)
}