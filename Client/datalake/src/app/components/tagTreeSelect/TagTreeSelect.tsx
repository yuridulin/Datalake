import {
	AccessType,
	BlockNestedTagInfo,
	BlockTagRelation,
	BlockTreeInfo,
	TagSimpleInfo,
} from '@/api/swagger/data-contracts'
import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import { TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useMemo } from 'react'

interface TagTreeSelectProps {
	blocks?: BlockTreeInfo[]
	tags?: TagSimpleInfo[]
	value?: [number, number?]
	onChange?: (value: [tagId: number, relationId: number]) => void
}

// Преобразовать BlockTreeInfo[] в формат, понятный Antd TreeSelect
function convertToTreeSelectNodes(
	blockTree: BlockTreeInfo[] | undefined,
	parentPath: string[] = [],
): DefaultOptionType[] {
	if (!blockTree) return []

	return blockTree.map((block) => {
		const currentPath = [...parentPath, block.name]
		const fullTitle = currentPath.join('.')

		return {
			title: (
				<>
					<BlockIcon /> {block.name}
				</>
			),
			value: -10000000 - block.id,
			key: `block-${block.id}`,
			selectable: false,
			fullTitle,
			data: block,
			children: [
				// все теги в этом блоке
				...block.tags.map((tag) => ({
					title: (
						<>
							<TagIcon type={tag.sourceType} /> {tag.localName} #{tag.id} <TagFrequencyEl frequency={tag.frequency} />
						</>
					),
					value: tag.relationId,
					key: `tag-${tag.id}-${tag.relationId}`,
					fullTitle: `${fullTitle}.${tag.localName}`,
					data: tag,
				})),
				// рекурсивно вложенные блоки
				...convertToTreeSelectNodes(block.children || undefined, currentPath),
			],
		}
	})
}

// Поиск узла по прямому relationId
function findNodeByValue(nodes: DefaultOptionType[], value: number): DefaultOptionType | null {
	for (const node of nodes) {
		if (node.value === value) return node
		if (node.children) {
			const found = findNodeByValue(node.children, value)
			if (found) return found
		}
	}
	return null
}

// Поиск первой доступной связи для заданного tagId
function findFirstNodeByTagId(nodes: DefaultOptionType[], tagId: number): DefaultOptionType | null {
	for (const node of nodes) {
		if (node.children) {
			const foundInChild = findFirstNodeByTagId(node.children, tagId)
			if (foundInChild) return foundInChild
		}
		if (node.data?.id === tagId) {
			return node
		}
	}
	return null
}

// Рекурсивная проверка наличия тега в дереве блоков
/* function isTagInTree(blocks: BlockTreeInfo[] | null | undefined, tagId: number): boolean {
	if (!blocks) return false
	for (const block of blocks) {
		if (block.tags.some(tag => tag.id === tagId)) return true
		if (isTagInTree(block.children, tagId)) return true
	}
	return false
} */

const TagTreeSelect: React.FC<TagTreeSelectProps> = ({ blocks = [], tags = [], value, onChange = () => {} }) => {
	// Формируем дерево с виртуальным блоком для нераспределенных тегов
	const treeData = useMemo(() => {
		// Получаем все ID тегов, которые есть в дереве (включая вложенные)
		const allTagIds = new Set<number>()
		const collectTagIds = (blocks: BlockTreeInfo[] | null | undefined) => {
			if (!blocks) return
			for (const block of blocks) {
				block.tags.forEach((tag) => allTagIds.add(tag.id))
				collectTagIds(block.children)
			}
		}
		collectTagIds(blocks)

		// Фильтруем теги, которых нет в дереве
		const orphanTags = tags.filter((tag) => !allTagIds.has(tag.id))

		// Виртуальный блок для нераспределенных тегов
		const virtualBlock: BlockTreeInfo = {
			id: 0,
			guid: 'virtual',
			name: 'Нераспределенные теги',
			fullName: 'Нераспределенные теги',
			tags: orphanTags.map((tag, index) => ({
				...tag,
				relationId: -(index + 1),
				relationType: BlockTagRelation.Static,
				localName: tag.name,
				sourceId: 0,
			})),
			children: [],
			accessRule: {
				access: AccessType.Manager,
				ruleId: 0,
			},
		}

		// Объединяем реальное дерево и виртуальный блок
		const treeSource = orphanTags.length ? [...blocks, virtualBlock] : blocks
		return convertToTreeSelectNodes(treeSource)
	}, [blocks, tags])

	// Вычисляем выбранное значение
	const selected = useMemo(() => {
		if (!value) return undefined

		const [tagId, relationId] = value

		// Поиск по точному relationId
		if (relationId) {
			const node = findNodeByValue(treeData, relationId)
			if (node) {
				return { value: node.value as number, label: node.fullTitle }
			}
		}

		// Поиск первого подходящего tagId
		if (tagId) {
			const node = findFirstNodeByTagId(treeData, tagId)
			if (node) {
				return { value: node.value as number, label: node.fullTitle }
			}
		}

		return undefined
	}, [value, treeData])

	// Обработчик изменения значения
	const handleChange = (sel: { value: number; label: React.ReactNode } | undefined) => {
		if (!sel) {
			onChange([0, 0])
			return
		}
		const node = findNodeByValue(treeData, sel.value)
		if (node?.data) {
			onChange([node.data.id, sel.value])
		} else {
			onChange([0, 0])
		}
	}

	// Функция фильтрации узлов
	const filterTreeNode = (input: string, treeNode: DefaultOptionType): boolean => {
		const searchText = input.toLowerCase()

		// Поиск по fullTitle (полному пути)
		if (treeNode.fullTitle && treeNode.fullTitle.toLowerCase().includes(searchText)) {
			return true
		}

		// Поиск по данным узла
		if (treeNode.data) {
			const data = treeNode.data

			// Поиск по ID
			if (typeof data.id === 'number' && data.id.toString().includes(searchText)) {
				return true
			}

			// Поиск по GUID
			if (data.guid && data.guid.toLowerCase().includes(searchText)) {
				return true
			}

			// Обработка блоков
			if ('fullName' in data) {
				const block = data as BlockTreeInfo
				if (
					(block.name && block.name.toLowerCase().includes(searchText)) ||
					(block.fullName && block.fullName.toLowerCase().includes(searchText))
				) {
					return true
				}
			}
			// Обработка тегов
			else {
				const tag = data as BlockNestedTagInfo
				if (
					(tag.name && tag.name.toLowerCase().includes(searchText)) ||
					(tag.localName && tag.localName.toLowerCase().includes(searchText)) ||
					(typeof tag.relationId === 'number' && tag.relationId.toString().includes(searchText))
				) {
					return true
				}
			}
		}

		return false
	}

	return (
		<TreeSelect
			treeData={treeData}
			value={selected}
			onChange={handleChange}
			showSearch
			allowClear
			labelInValue
			filterTreeNode={filterTreeNode}
			placeholder='Выберите тег'
			treeDefaultExpandAll
			style={{ width: '100%' }}
			dropdownStyle={{ maxHeight: 400, overflow: 'auto' }}
		/>
	)
}

export default TagTreeSelect
