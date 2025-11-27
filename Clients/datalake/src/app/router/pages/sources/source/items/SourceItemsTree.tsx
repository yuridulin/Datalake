import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagType } from '@/generated/data-contracts'
import { DownOutlined } from '@ant-design/icons'
import { Tree, theme } from 'antd'
import { SourceItemsTreeLeaf } from './SourceItemsTreeLeaf'
import { GroupedEntry, TreeNodeData } from './utils/SourceItems.types'
import { formatCount } from './utils/SourceItems.utils'

type SourceItemsTreeProps = {
	groups: GroupedEntry[]
	onCreateTag: (item: string, tagType: TagType) => void
	onDeleteTag: (tagId: number) => void
}

export const SourceItemsTree = ({ groups, onCreateTag, onDeleteTag }: SourceItemsTreeProps) => {
	const { token } = theme.useToken()

	const buildTree = (groups: GroupedEntry[]): TreeNodeData[] => {
		// Интерфейс для временного узла при построении дерева
		interface TempTreeNode {
			children: Record<string, TempTreeNode>
			countLeaves: number
			countTags: number
			path: string
			group?: GroupedEntry
			isLeaf?: boolean
		}

		// создаём единый корень
		const root: TempTreeNode = {
			children: {},
			countLeaves: 0,
			countTags: 0,
			path: '',
		}

		groups.forEach((group) => {
			const parts = group.path === '__no_path__' ? ['Без пути'] : group.path.split('.')
			let current = root

			parts.forEach((part, idx) => {
				if (!current.children[part]) {
					current.children[part] = {
						children: {},
						countLeaves: 0,
						countTags: 0,
						path: parts.slice(0, idx + 1).join('.'),
					}
				}
				current = current.children[part]
			})

			// leaf: сохраняем инфо
			current.group = group
			current.isLeaf = true
			current.countLeaves = 1
			current.countTags = group.tagInfoArray.length
		})

		// рекурсивная сборка дерева
		const buildNode = (node: TempTreeNode, keyPath: string): TreeNodeData[] => {
			return Object.entries(node.children).map(([name, child]) => {
				const fullKey = `${keyPath}.${name}`
				const isLeaf = !!child.isLeaf

				// считаем суммарные метрики
				const sub = buildNode(child, fullKey)
				const leavesCount = sub.reduce((sum, n) => sum + n.countLeaves, 0) + (child.isLeaf ? 1 : 0)
				const tagsCount = sub.reduce((sum, n) => sum + n.countTags, 0) + (child.countTags || 0)

				// заголовок: если лист, на первой строке — value
				const title = isLeaf ? (
					<div style={{ display: 'flex', flexDirection: 'row' }}>
						{/* 2) Имя узла */}
						<span>{name}</span>
						&emsp;
						{/* 1) Значение */}
						{child.group?.itemInfo && (
							<TagCompactValue
								type={child.group.itemInfo.type}
								quality={child.group.itemInfo.value.quality}
								record={child.group.itemInfo.value}
							/>
						)}
					</div>
				) : (
					<div style={{ display: 'flex', alignItems: 'center' }}>
						<span style={{ flex: 1 }}>{name}</span>&emsp;
						<span style={{ color: token.colorTextSecondary, fontSize: '0.85em' }}>
							значений{formatCount(leavesCount)}, тегов{formatCount(tagsCount)}
						</span>
					</div>
				)

				return {
					key: fullKey,
					title,
					path: child.path,
					isLeaf,
					countLeaves: leavesCount,
					countTags: tagsCount,
					children: sub.length > 0 ? sub : undefined,
					group: child.group, // Сохраняем группу в узле
				}
			})
		}

		return buildNode(root, 'root')
	}

	const renderLeafContent = (group?: GroupedEntry) => {
		if (!group) return null
		return <SourceItemsTreeLeaf group={group} onCreateTag={onCreateTag} onDeleteTag={onDeleteTag} />
	}

	return (
		<Tree
			showLine
			switcherIcon={<DownOutlined />}
			treeData={buildTree(groups)}
			titleRender={(node) => (
				<div>
					{node.title}
					{node.isLeaf && renderLeafContent(node.group)}
				</div>
			)}
		/>
	)
}
