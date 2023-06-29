import { useEffect, useState } from "react"
import { useUpdateContext } from "../../context/updateContext"
import { useInterval } from "../../hooks/useInterval"
import axios from "axios"
import { Block } from "../../@types/block"
import TreeSub, { TreeSubProps } from "../small/TreeSub"
import { useFetching } from "../../hooks/useFetching"
import { Navigate } from "react-router-dom"

export default function Tree() {

	const { lastUpdate } = useUpdateContext()
	const [ blocks, setBlocks ] = useState([] as TreeSubProps[])

	const [ load,, error ] = useFetching(async ()=> {
		let res = await axios.get('blocks/list')
		setBlocks(res.data.map((x: Block) => blockToTree(x)))
	})

	function blockToTree(block: Block): TreeSubProps {
		return {
			icon: 'data_object',
			link: '/blocks/' + block.Id,
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
			<TreeSub icon="dashboard" text="Панель состояния" link="/" />
			<TreeSub icon="slideshow" text="Просмотр значений" link="/viewer" />
			<br />
			<TreeSub icon="input" text="Источники" link="/sources" />
			<TreeSub icon="inventory" text="Теги" link="/tags" />
			<TreeSub icon="data_object" text="Объекты" link="/blocks" elements={blocks} />
		</div>
	)
}