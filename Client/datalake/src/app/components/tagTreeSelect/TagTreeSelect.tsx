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
			value: -1000000 - block.id, // специально берем большое значение, чтобы не конфликтовать с фейковыми связями
			selectable: false,
			fullTitle,
			data: block,
			children: [
				...block.tags.map((tag) => ({
					title: (
						<>
							<TagIcon type={tag.sourceType} /> {tag.localName} #{tag.id} <TagFrequencyEl frequency={tag.frequency} />
						</>
					),
					value: tag.relationId,
					fullTitle: `${fullTitle}.${tag.localName}`,
					data: tag,
				})),
				...convertToTreeSelectNodes(block.children || undefined, currentPath),
			],
		}
	})
}

// Поиск узла по relationId
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

// Поиск тега по tagId
function findFirstNodeByTagId(nodes: DefaultOptionType[], tagId: number): DefaultOptionType | null {
	for (const node of nodes) {
		if (node.children) {
			const foundInChild = findFirstNodeByTagId(node.children, tagId)
			if (foundInChild) return foundInChild
		}
		if (node.data?.relationId !== undefined && node.data.id === tagId) {
			return node
		}
	}
	return null
}

const TagTreeSelect: React.FC<TagTreeSelectProps> = ({ blocks = [], tags = [], value, onChange = () => {} }) => {
	// Формируем дерево с виртуальным блоком для нераспределенных тегов
	const treeData = useMemo(() => {
		// Собираем все ID тегов в дереве (включая вложенные)
		const allTagIds = new Set<number>()
		const collectTagIds = (blocks: BlockTreeInfo[] | null | undefined) => {
			if (!blocks) return
			for (const block of blocks) {
				block.tags.forEach((tag) => allTagIds.add(tag.id))
				collectTagIds(block.children)
			}
		}
		collectTagIds(blocks)

		// Фильтруем теги, отсутствующие в дереве
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
			if (node) return { value: node.value as number, label: node.fullTitle }
		}

		// Поиск первого подходящего tagId
		if (tagId) {
			const node = findFirstNodeByTagId(treeData, tagId)
			if (node) return { value: node.value as number, label: node.fullTitle }
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

		// Поиск по полному пути
		if (treeNode.fullTitle?.toLowerCase().includes(searchText)) return true

		// Поиск по данным узла
		if (treeNode.data) {
			const data = treeNode.data

			// Поиск по ID
			if (data.id?.toString().includes(searchText)) return true

			// Поиск по GUID
			if (data.guid?.toLowerCase().includes(searchText)) return true

			// Блоки
			if ('fullName' in data) {
				const block = data as BlockTreeInfo
				if (block.name?.toLowerCase().includes(searchText) || block.fullName?.toLowerCase().includes(searchText))
					return true
			}
			// Теги
			else if ('relationId' in data) {
				const tag = data as BlockNestedTagInfo
				if (
					tag.name?.toLowerCase().includes(searchText) ||
					tag.localName?.toLowerCase().includes(searchText) ||
					tag.relationId?.toString().includes(searchText)
				)
					return true
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
