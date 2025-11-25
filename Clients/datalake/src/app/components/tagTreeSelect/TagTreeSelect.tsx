import {
	convertToTreeSelectNodes,
	createFullTree,
	decodeBlockTagPair,
	encodeBlockTagPair,
	filterTreeNode,
} from '@/app/components/tagTreeSelect/treeSelectShared'
import { logger } from '@/services/logger'
import { BlockTreeInfo, TagSimpleInfo } from '@/generated/data-contracts'
import { theme, TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useMemo } from 'react'

interface TagTreeSelectProps {
	blocks?: BlockTreeInfo[]
	tags?: TagSimpleInfo[]
	value?: [number | null | undefined, number]
	onChange?: (value: [blockId: number, tagId: number]) => void
}

function findNodeByValue(nodes: DefaultOptionType[], value: string): DefaultOptionType | null {
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
		const [blockId, tagId] = value
		logger.debug('TagTreeSelect selected', { component: 'TagTreeSelect', value, blockId, tagId })

		const node =
			findNodeByValue(treeData, encodeBlockTagPair(blockId ?? 0, tagId)) ?? findFirstNodeByTagId(treeData, tagId)
		if (node) return { value: node.value as string, label: node.fullTitle }

		return undefined
	}, [value, treeData])

	const handleChange = (sel: { value: string; label: React.ReactNode } | undefined) => {
		logger.debug('TagTreeSelect change', { component: 'TagTreeSelect', value: sel?.value })
		if (!sel) {
			onChange([0, 0])
			return
		}

		const { blockId, tagId } = decodeBlockTagPair(sel.value)
		onChange([blockId, tagId])
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
