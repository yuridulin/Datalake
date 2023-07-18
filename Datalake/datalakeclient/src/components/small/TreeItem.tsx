import { NavLink } from "react-router-dom"

type TreeItemProps = {
	icon: string
	to: string
	text: string
	open?: boolean
	toggle?: React.MouseEventHandler
}

export default function TreeItem({ icon, to, text, open, toggle }: TreeItemProps) {

	return (
		<NavLink to={to} className="tree-item">
			{icon && <i className="material-icons">{icon}</i>}
			<span>{text}</span>
			{(!!toggle) && <i className="material-icons tree-arrow" onClick={e => { e.preventDefault(); e.stopPropagation(); toggle(e); }}>{open ? 'expand_more' : 'chevron_right'}</i>}
		</NavLink>
	)
}