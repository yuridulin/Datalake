import { NavLink } from "react-router-dom"

type TreeItemProps = {
	icon: string
	link: string | null
	text: string
	withArrow: boolean
}

export default function TreeItem({ icon, link = null, text, withArrow = false }: TreeItemProps) {
	return (
		link
		? <NavLink to={link} className="tree-item">
			{withArrow && <i className="material-icons tree-arrow">chevron_right</i>}
			{icon && <i className="material-icons">{icon}</i>}
			<span>{text}</span>
		</NavLink>
		: <span className="tree-item">
			{withArrow && <i className="material-icons tree-arrow">chevron_right</i>}
			{icon && <i className="material-icons">{icon}</i>}
			<span>{text}</span>
		</span>
	)
}