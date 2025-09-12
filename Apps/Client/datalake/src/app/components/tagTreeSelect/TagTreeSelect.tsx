import {
	convertToTreeSelectNodes,
	createFullTree,
	decodeBlockTagPair,
	encodeBlockTagPair,
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
	onChange?: (value: [tagId: number, blockId: number]) => void
}

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

const TagTreeSelect: React.FC<TagTreeSelectProps> = ({ blocks = [], tags = [], value, onChange = () => {} }) => {
	const { token } = theme.useToken()

	const treeData = useMemo(() => {
		const fullTree = createFullTree([blocks, tags])
		return convertToTreeSelectNodes(fullTree, undefined, token)
	}, [blocks, tags, token])

	const selected = useMemo(() => {
		if (!value) return undefined
		const [tagId, blockId] = value

		if (blockId) {
			const value = encodeBlockTagPair(blockId, tagId)
			const node = findNodeByValue(treeData, value)
			if (node) return { value: node.value as number, label: node.fullTitle }
		}

		if (tagId) {
			const node = findFirstNodeByTagId(treeData, tagId)
			if (node) return { value: node.value as number, label: node.fullTitle }
		}

		return undefined
	}, [value, treeData])

	const handleChange = (sel: { value: number; label: React.ReactNode } | undefined) => {
		if (!sel) {
			onChange([0, 0])
			return
		}

		if (sel.value < 0) {
			onChange([-sel.value, 0])
		} else {
			const { blockId, tagId } = decodeBlockTagPair(sel.value)
			onChange([tagId, blockId])
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
