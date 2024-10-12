import { Button, theme, Tree, TreeDataNode, TreeProps } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { BlockTreeInfo } from '../../../api/swagger/data-contracts'
import compareValues from '../../../hooks/compareValues'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'

function transformBlockTreeInfo(blocks: BlockTreeInfo[]): TreeDataNode[] {
	const data = blocks.map((block) => {
		const transformedBlock: TreeDataNode = {
			key: block.id,
			title: block.name,
		}

		transformedBlock.children = transformBlockTreeInfo(block.children)
		if (transformedBlock.children.length === 0) {
			transformedBlock.isLeaf = true
		}

		return transformedBlock
	})

	return data.sort((a, b) => compareValues(String(a.title), String(b.title)))
}

export default function BlocksMover() {
	const [blocks, setBlocks] = useState([] as TreeDataNode[])
	const [loading, setLoading] = useState(false)
	const { token } = theme.useToken()

	function load() {
		api.blocksReadAsTree().then((res) =>
			setBlocks(transformBlockTreeInfo(res.data)),
		)
	}

	const findParentKey = (
		data: TreeDataNode[],
		key: React.Key,
	): React.Key | null => {
		for (let i = 0; i < data.length; i++) {
			if (data[i].children) {
				for (let j = 0; j < data[i].children!.length; j++) {
					if (data[i].children![j].key === key) {
						return data[i].key
					}
					const parentKey = findParentKey(data[i].children!, key)
					if (parentKey) {
						return parentKey
					}
				}
			}
		}
		return null
	}

	const onDrop: TreeProps['onDrop'] = (info) => {
		const dropKey = info.node.key
		const dragKey = info.dragNode.key

		const loop = (
			data: TreeDataNode[],
			key: React.Key,
			callback: (
				node: TreeDataNode,
				i: number,
				data: TreeDataNode[],
			) => void,
		) => {
			for (let i = 0; i < data.length; i++) {
				if (data[i].key === key) {
					return callback(data[i], i, data)
				}
				if (data[i].children) {
					loop(data[i].children!, key, callback)
				}
			}
		}
		const data = [...blocks]

		let dragObj: TreeDataNode

		loop(data, dragKey, (item, index, arr) => {
			arr.splice(index, 1)
			dragObj = item
		})

		if (!info.dropToGap) {
			loop(data, dropKey, (item) => {
				item.children = item.children || []
				item.children.unshift(dragObj)
				item.isLeaf = false // Обновляем флаг isLeaf
			})
		} else {
			let ar: TreeDataNode[] = []
			let i: number
			loop(data, dropKey, (_item, index, arr) => {
				ar = arr
				i = index
			})
			ar.splice(i! + (info.dropPosition === -1 ? 0 : 1), 0, dragObj!)
		}

		const parentKey = findParentKey(data, dragKey)

		setLoading(true)
		api.blocksMove(Number(info.dragNode.key), {
			parentId: Number(parentKey),
		})
			.then(() => {
				setBlocks(data)
				setLoading(false)
			})
			.catch(() => {
				setLoading(false)
			})

		setBlocks(data)
	}

	useEffect(load, [])

	console.log(token)

	return (
		<>
			<PageHeader
				left={
					<NavLink to={routes.blocks.root}>
						<Button>Вернуться</Button>
					</NavLink>
				}
			>
				Иерархия блоков
			</PageHeader>
			<Tree
				draggable
				disabled={loading}
				blockNode
				style={{
					fontSize: '1.1em',
					backgroundColor: token.colorBgContainer,
				}}
				onDrop={onDrop}
				treeData={blocks}
			/>
		</>
	)
}
