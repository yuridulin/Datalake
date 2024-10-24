import { Button, theme, Tree, TreeDataNode, TreeProps } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import compareValues from '../../../api/extensions/compareValues'
import api from '../../../api/swagger-api'
import { UserGroupTreeInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'
import UserGroupsCreateModal from './usergroup/modals/UserGroupsCreateModal'

function transformToTreeNode(groups: UserGroupTreeInfo[]): TreeDataNode[] {
	const data = groups.map((group) => {
		const transformedBlock: TreeDataNode = {
			key: group.guid,
			title: group.name,
		}

		transformedBlock.children = transformToTreeNode(group.children)
		if (transformedBlock.children.length === 0) {
			transformedBlock.isLeaf = true
		}

		return transformedBlock
	})

	return data.sort((a, b) => compareValues(String(a.title), String(b.title)))
}

export default function UserGroupsTreeMove() {
	const [tree, setTree] = useState([] as TreeDataNode[])
	const [loading, setLoading] = useState(false)
	const { token } = theme.useToken()

	function load() {
		api.userGroupsReadAsTree().then((res) =>
			setTree(transformToTreeNode(res.data)),
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
		const data = [...tree]

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
		api.userGroupsMove(String(info.dragNode.key), {
			parentGuid: parentKey == null ? null : String(parentKey),
		})
			.then(() => {
				setTree(data)
				setLoading(false)
			})
			.catch(() => {
				setLoading(false)
			})
	}

	useEffect(load, [])

	return (
		<>
			<PageHeader
				right={
					<>
						<NavLink to={routes.userGroups.list}>
							<Button>Вернуться к списку</Button>
						</NavLink>
						&ensp;
						<UserGroupsCreateModal onCreate={load} />
					</>
				}
			>
				Группы пользователей
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
				treeData={tree}
			/>
		</>
	)
}
