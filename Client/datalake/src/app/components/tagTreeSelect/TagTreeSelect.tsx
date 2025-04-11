import {
	AccessType,
	BlockSimpleInfo,
	BlockTagRelation,
	BlockTreeInfo,
	TagSimpleInfo,
} from '@/api/swagger/data-contracts'
import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useEffect, useState } from 'react'

interface TagTreeSelectProps {
	blocks?: BlockTreeInfo[]
	tags?: TagSimpleInfo[]
	value?: number
	onChange?: (value: number) => void
}

const flattenNestedTags = (
	blockTree: BlockTreeInfo[],
	parentNames: BlockSimpleInfo[] = [],
): FlattenedNestedTagsType => {
	let mapping: FlattenedNestedTagsType = {}
	blockTree.forEach((block) => {
		const currentParents = [...parentNames, { ...block }]
		block.tags.forEach((tag) => {
			mapping[tag.id] = {
				...tag,
				parents: currentParents,
			}
		})
		const childrenMapping = flattenNestedTags(block.children, currentParents)
		mapping = { ...mapping, ...childrenMapping }
	})

	return mapping
}

const convertToTreeSelectNodes = (blockTree: BlockTreeInfo[], parentPath: string[] = []): DefaultOptionType[] => {
	return blockTree.map((block) => {
		const currentPath = [...parentPath, block.name]
		return {
			title: (
				<>
					<BlockIcon />
					&ensp;{block.name}
				</>
			),
			fullTitle: block.name,
			value: -block.id,
			key: -block.id,
			selectable: false,
			children: [
				...block.tags.map((tag) => ({
					title: (
						<>
							<TagIcon type={tag.sourceType} />
							&ensp;
							{tag.localName}&ensp;
							<TagFrequencyEl frequency={tag.frequency} />
						</>
					),
					fullTitle: `${currentPath.join('.')}.${tag.localName}`,
					value: tag.id,
					key: tag.id.toString(),
				})),
				...convertToTreeSelectNodes(block.children, currentPath),
			],
		}
	})
}

const findNodeByValue = (nodes: DefaultOptionType[], value: number): DefaultOptionType | null => {
	for (const node of nodes) {
		if (node.value === value) {
			return node
		}
		if (node.children) {
			const found = findNodeByValue(node.children, value)
			if (found) {
				return found
			}
		}
	}
	return null
}

type SelectValue = { value: number; label: React.ReactNode } | undefined

const TagTreeSelect: React.FC<TagTreeSelectProps> = ({ blocks = [], tags = [], value, onChange = () => {} }) => {
	const [treeData, setTreeData] = useState<DefaultOptionType[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>({})
	const [searchValue, setSearchValue] = useState<string>('')
	const [selected, setSelected] = useState<SelectValue>(undefined)

	useEffect(() => {
		const mappingFromBlocks = flattenNestedTags(blocks)
		const notUsedTags = tags.filter((tag) => !mappingFromBlocks[tag.id])
		if (notUsedTags.length > 0) {
			const fakeBlock: BlockTreeInfo = {
				id: 0,
				guid: 'fake',
				name: 'Нераспределенные теги',
				fullName: 'Нераспределенные теги',
				tags: notUsedTags
					.map((tag) => ({
						...tag,
						relation: BlockTagRelation.Static,
						localName: tag.name,
						sourceId: 0,
					}))
					.sort((a, b) => a.localName.localeCompare(b.localName)),
				children: [],
				accessRule: {
					ruleId: 0,
					accessType: AccessType.Manager,
				},
			}
			blocks.push(fakeBlock)

			setTagMapping({
				...mappingFromBlocks,
				...flattenNestedTags([fakeBlock]),
			})
		} else {
			setTagMapping(mappingFromBlocks)
		}
		const data = convertToTreeSelectNodes(blocks)
		setTreeData(data)

		if (value === 0 || value === undefined) {
			setSelected(undefined)
		} else if (!selected || selected.value !== value) {
			const node = findNodeByValue(data, value)
			setSelected({
				value: value,
				label: node ? node.fullTitle : 'Неизвестный тег',
			})
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [blocks])

	const filterTreeNode = (inputValue: string, treeNode: DefaultOptionType): boolean => {
		const search = inputValue.toLowerCase()
		const nodeValue = treeNode.value as number

		if (nodeValue < 0) {
			return treeNode.title?.toString().toLowerCase().includes(search) ?? false
		}

		const tagInfo = tagMapping[nodeValue]
		if (!tagInfo) return false

		const searchFields = [tagInfo.localName, tagInfo.name, tagInfo.guid, String(nodeValue), treeNode.title?.toString()]

		return searchFields.some((field) => field?.toLowerCase().includes(search))
	}

	return (
		<TreeSelect
			treeData={treeData}
			value={selected}
			onChange={(newValue) => {
				const node = findNodeByValue(treeData, newValue.value)
				const newLabel = node ? node.fullTitle : newValue.label
				const newSelected = { value: newValue.value, label: newLabel }
				setSelected(newSelected)
				onChange(newValue.value)
			}}
			showSearch
			searchValue={searchValue}
			onSearch={setSearchValue}
			autoClearSearchValue={false}
			filterTreeNode={filterTreeNode}
			labelInValue
			allowClear
			treeDefaultExpandAll
			style={{ width: '100%' }}
			dropdownStyle={{ maxHeight: 400, overflow: 'auto' }}
			placeholder='Выберите тег'
		/>
	)
}

export default TagTreeSelect
