import NoAccessEl from '@/app/components/NoAccessEl'
import PageHeader from '@/app/components/PageHeader'
import compareValues from '@/functions/compareValues'
import { AccessType, UserGroupTreeInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, theme, Tree, TreeDataNode, TreeProps } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import routes from '../../routes'
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

const UserGroupsTreeMove = observer(() => {
	const store = useAppStore()
	const [tree, setTree] = useState([] as TreeDataNode[])
	const [loading, setLoading] = useState(false)
	const { token } = theme.useToken()

	function load() {
		store.api.userGroupsGetTree().then((res) => setTree(transformToTreeNode(res.data)))
	}

	const findParentKey = (data: TreeDataNode[], key: React.Key): React.Key | null => {
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
			callback: (node: TreeDataNode, i: number, data: TreeDataNode[]) => void,
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
		store.api
			.userGroupsMove(String(info.dragNode.key), {
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

	if (!store.hasGlobalAccess(AccessType.Admin)) return <NoAccessEl />

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
})

export default UserGroupsTreeMove
