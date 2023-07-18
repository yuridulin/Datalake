import { useEffect, useState } from "react"
import { useUpdateContext } from "../../context/updateContext"
import { useInterval } from "../../hooks/useInterval"
import axios from "axios"
import { BlockType } from "../../@types/BlockType"
import { useFetching } from "../../hooks/useFetching"
import { Navigate } from "react-router-dom"
import TreeSub, { TreeSubProps } from "../small/TreeSub"
import TreeItem from "../small/TreeItem"

export default function Tree() {

	const { lastUpdate } = useUpdateContext()
	const [ blocks, setBlocks ] = useState([] as TreeSubProps[])

	const [ load,, error ] = useFetching(async ()=> {
		let res = await axios.get('blocks/list')
		setBlocks(res.data.map((x: BlockType) => blockToTree(x)))
	})

	function blockToTree(block: BlockType): TreeSubProps {
		return {
			id: String(block.Id),
			icon: 'data_object',
			to: '/blocks/view/' + block.Id,
			text: block.Name,
			elements: block.Children.map(x => blockToTree(x))
		}
	}

	// eslint-disable-next-line
	useEffect(() => { load() }, [lastUpdate])
	useInterval(load, 10000)

	return (
		error
		? <Navigate to="/offline" />
		: <div className="tree">
			<TreeItem to="/" icon="dashboard" text="Панель состояния" />
			<TreeItem to="/viewer" icon="slideshow" text="Просмотр значений" />
			<br />
			<TreeItem to="/sources" icon="input" text="Источники" />
			<TreeItem to="/tags" icon="inventory" text="Теги" />
			<br />
			<TreeSub id="0" icon="data_object" text="Объекты" to="/blocks" elements={blocks} />
		</div>
	)
}