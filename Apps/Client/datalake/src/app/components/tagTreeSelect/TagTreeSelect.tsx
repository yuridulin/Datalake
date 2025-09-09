import {
	convertToTreeSelectNodes,
	createFullTree,
	filterTreeNode,
} from '@/app/components/tagTreeSelect/treeSelectShared'
import { BlockTreeInfo, TagSimpleInfo } from '@/generated/data-contracts'
import { theme, TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useMemo } from 'react'

interface TagTreeSelectProps {
	blocks?: BlockTreeInfo[]
	tags?: TagSimpleInfo[]
	value?: [number, number | null | undefined]
	onChange?: (value: [tagId: number, relationId: number]) => void
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
	const { token } = theme.useToken()

	// Формируем дерево с виртуальным блоком для нераспределенных тегов
	const treeData = useMemo(() => {
		const fullTree = createFullTree([blocks, tags])
		return convertToTreeSelectNodes(fullTree, undefined, token)
	}, [blocks, tags, token])

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
			styles={{ popup: { root: { maxHeight: 400, overflow: 'auto' } } }}
		/>
	)
}

export default TagTreeSelect
