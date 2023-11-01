import { useEffect, useState } from "react"
import { useUpdateContext } from "../../context/updateContext"
import { useInterval } from "../../hooks/useInterval"
import axios from "axios"
import { useFetching } from "../../hooks/useFetching"
import { NavLink, Navigate, useLocation, useNavigate } from "react-router-dom"
import { TreeItem } from "../../@types/TreeItem"
import { Dropdown, Tree } from 'antd'
import type { DataNode } from 'antd/es/tree'
import { TreeType } from "../../@types/enums/treeType"
import router from "../../router/router"
import { BlockType } from "../../@types/BlockType"
import { items } from "./TreeContextMenu"

export default function TreeContainer() {

	const location = useLocation()
	const navigate = useNavigate()
	const { lastUpdate, setUpdate, setCheckedTags } = useUpdateContext()
	const [ elements, setElements ] = useState([] as TreeItem[])
	const [ treeData, setTreeData ] = useState([] as DataNode[])
	const [ blocks, setBlocks ] = useState([] as DataNode[])

	// рекурсивные функции для сбора дерева
	function getElements(treeItems: TreeItem[]): TreeItem[] {
		let items = [ ...treeItems ]
		treeItems.forEach(x => items = [ ...items, ...getElements(x.Items) ])
		return items
	}
	
	function treeItemToDataNode(treeItems: TreeItem[]): DataNode[] {
		return treeItems.filter(x => x.Type !== TreeType.Link).map(x => ({
			title: x.Name,
			key: x.FullName,
			children: treeItemToDataNode(x.Items),
		}))
	}

	function blockToDataNode(blocks: BlockType[]): DataNode[] {
		return blocks.map(x => ({
			title: x.Name,
			key: x.Id,
			children: blockToDataNode(x.Children)
		}))
	}

	// контекстное меню

	const [ loadLink ] = useFetching(async (url: string) => {
		let res = await axios.get(url)
		if (res.data.Done) setUpdate(new Date())
	})

	// компонент дерева

	const [expandedKeys, setExpandedKeys] = useState<React.Key[]>()
	const [checkedKeys, setCheckedKeys] = useState<React.Key[]>([])
	const [autoExpandParent, setAutoExpandParent] = useState<boolean>(true)

	const onExpand = (expandedKeysValue: React.Key[]) => {
		console.log('onExpand', expandedKeysValue)
		setExpandedKeys(expandedKeysValue)
		localStorage.setItem('expandedKeys', expandedKeysValue.join('|'))
		setAutoExpandParent(false)
	}

	const onCheck = (checkedKeysValue: React.Key[]) => {
		setCheckedKeys(checkedKeysValue)
		setCheckedTags(checkedKeysValue as string[])

		// переход на страницу с выбранными тегами и возврат, если ничего не выбрано
		if (checkedKeysValue.length > 0 && location.pathname !== '/tags/selected/') {
			router.navigate('/tags/selected/')
		} else if (checkedKeysValue.length === 0 && location.pathname === '/tags/selected/') {
			navigate(-1)
		}
	}

	// рендер-функции

	const TreeElement = (nodeData: DataNode) => {
		let currents = elements.filter(x => x.FullName === nodeData.key)
		if (currents.length === 0) return
		let el = currents[0]
		
		if (el.Type === TreeType.TagGroup) {
			return <span className="tree-el">{el.Name}</span>
		}
		else {
			let root = el.Type === TreeType.Block ? 'blocks' :
				el.Type === TreeType.Source ? 'sources' :
				el.Type === TreeType.Tag ? 'tags':
				el.Name
			
			return <NavLink className="tree-el" to={`/${root}/${el.Id}`}>{el.Name}</NavLink>
		}
	}

	const BlockElement = (nodeData: DataNode) => {
		return <NavLink className="tree-el" to={`/blocks/${nodeData.key}`}>{String(nodeData.title)}</NavLink>
	}

	// загрузка данных

	const [ loadTags,, error ] = useFetching(async () => {
		let res = await axios.get('console/tree')
		let items = getElements(res.data)
		setElements(items)
		setTreeData(treeItemToDataNode(res.data))
	})

	const [ loadBlocks ] = useFetching(async () => {
		let res = await axios.get('blocks/list')
		setBlocks(blockToDataNode(res.data as BlockType[]))
	})

	function loader() {
		loadTags()
		loadBlocks()
	}

	// eslint-disable-next-line
	useEffect(loader, [lastUpdate])
	useInterval(loader, 10000)

	return (
		error
		? <Navigate to="/offline" />
		: <Dropdown menu={{ items: items, onClick: e => loadLink(e.key) }} trigger={['contextMenu']}>
			<div className="tree">
				<Tree
					checkable
					onExpand={onExpand}
					// инициализация ранее открытых элементов, работает только вот так. Хранение в localStorage
					expandedKeys={expandedKeys ?? localStorage.getItem('expandedKeys')?.split('|') ?? []}
					autoExpandParent={autoExpandParent}
					onCheck={(checked) => onCheck(checked as React.Key[])}
					checkedKeys={checkedKeys}
					treeData={treeData}
					titleRender={TreeElement}
				/>
				<Tree
					treeData={blocks}
					titleRender={BlockElement}
				/>
			</div>
		</Dropdown>
	)
}