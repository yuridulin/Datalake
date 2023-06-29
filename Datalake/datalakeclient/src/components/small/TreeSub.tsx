import TreeItem from "./TreeItem"

export type TreeSubProps = {
	icon: string
	link?: string | null
	text: string
	elements?: TreeSubProps[]
}

export default function TreeSub({ icon, link = null, text, elements = [] }: TreeSubProps) {
	return (
		elements.length === 0
		? <TreeItem withArrow={false} icon={icon} text={text} link={link} />
		: <div className="tree-block">
			<TreeItem withArrow={true} icon={icon} text={text} link={link} />	
			<div className="tree-sub">
				{elements.map(x => <TreeSub {...x} />)}
			</div>
		</div>
	)
}