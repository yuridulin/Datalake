import { NavLink } from "react-router-dom"
import { TreeItem } from "../../@types/TreeItem"
import { TreeType } from "../../@types/enums/treeType"

export type TreeLinkProps = {
	item: TreeItem,
	arrow?: boolean,
	open?: boolean,
	toggle?: React.MouseEventHandler,
}

export default function TreeLink({ item, arrow, open, toggle }: TreeLinkProps) {

	switch (item.Type) {
		case TreeType.Source:
			return (
				<div className="tree-item">
					<span className="tree-icon">
						<i className="material-icons">wb_cloudy</i>
					</span>
					<NavLink to={`/sources/${item.Id}`}>{item.Name}</NavLink>
					{arrow && 
					<span className="tree-icon" onClick={toggle}>
						<i className="material-icons">{open ? 'expand_more' : 'expand_less'}</i>
					</span>}
				</div>
			)

		case TreeType.TagGroup:
			return (
				<div className="tree-item">
					<span className="tree-icon">
						<i className="material-icons">style</i>
					</span>
					<NavLink to={`/sources/${item.Id}`}>{item.Name}</NavLink>
					{arrow && 
					<span className="tree-icon" onClick={toggle}>
						<i className="material-icons">{open ? 'expand_more' : 'expand_less'}</i>
					</span>}
				</div>
			)

		case TreeType.Tag:
			return (
				<div className="tree-item">
					<span className="tree-icon">
						<i className="material-icons">local_offer</i>
					</span>
					<NavLink to={`/tags/${item.Id}`}>{item.Name}</NavLink>
				</div>
			)
			
		case TreeType.Block:
			return (
				<div className="tree-item">
					<span className="tree-icon">
						<i className="material-icons">video_label</i>
					</span>
					<NavLink to={`/blocks/${item.Id}`}>{item.Name}</NavLink>
				</div>
			)

		default:
			return <></>
	}
}