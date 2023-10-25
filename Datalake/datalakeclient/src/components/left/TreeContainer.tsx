import { useEffect, useState } from "react"
import { useUpdateContext } from "../../context/updateContext"
import { useInterval } from "../../hooks/useInterval"
import axios from "axios"
import { useFetching } from "../../hooks/useFetching"
import { NavLink, Navigate, useLocation, useNavigate } from "react-router-dom"
import { TreeItem } from "../../@types/TreeItem"
import { Tree } from 'antd'
import type { DataNode } from 'antd/es/tree'
import { TreeType } from "../../@types/enums/treeType"
import router from "../../router/router"

export default function TreeContainer() {

	const location = useLocation()
	const navigate = useNavigate()
	const { lastUpdate, setCheckedTags } = useUpdateContext()
	const [ elements, setElements ] = useState([] as TreeItem[])
	const [ treeData, setTreeData ] = useState([] as DataNode[])

	const [ load,, error ] = useFetching(async () => {
		let res = await axios.get('console/tree')
		setElements(getElements(res.data))
		setTreeData(treeItemToDataNode(res.data))
	})

	function getElements(treeItems: TreeItem[]): TreeItem[] {
		let items = [ ...treeItems ]
		treeItems.forEach(x => items = [ ...items, ...getElements(x.Items) ])
		return items
	}
	
	function treeItemToDataNode(treeItems: TreeItem[]): DataNode[] {
		return treeItems.map(x => ({
			title: x.Name,
			key: x.FullName,
			children: treeItemToDataNode(x.Items),
		}))
	}

	const [expandedKeys, setExpandedKeys] = useState<React.Key[]>()
	const [checkedKeys, setCheckedKeys] = useState<React.Key[]>([])
	const [selectedKeys, setSelectedKeys] = useState<React.Key[]>([])
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

	const onSelect = (selectedKeysValue: React.Key[], info: any) => {
		console.log('onSelect', info);
		setSelectedKeys(selectedKeysValue);
	}

	const TreeElement = (nodeData: DataNode) => {
		let el = elements.filter(x => x.FullName === nodeData.key)[0]
		
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

	// eslint-disable-next-line
	useEffect(() => { load() }, [lastUpdate])
	useInterval(load, 10000)

	return (
		error
		? <Navigate to="/offline" />
		: <div className="tree">
			<Tree
				checkable
				onExpand={onExpand}
				// инициализация ранее открытых элементов, работает только вот так. Хранение в localStorage
				expandedKeys={expandedKeys ?? localStorage.getItem('expandedKeys')?.split('|') ?? []}
				autoExpandParent={autoExpandParent}
				onCheck={(checked) => onCheck(checked as React.Key[])}
				checkedKeys={checkedKeys}
				onSelect={onSelect}
				selectedKeys={selectedKeys}
				treeData={treeData}
				titleRender={TreeElement}
				/>
		</div>
	)
}