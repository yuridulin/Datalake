import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import getTagResolutionName from '@/functions/getTagResolutionName'
import {
	AccessType,
	BlockNestedTagInfo,
	BlockSimpleInfo,
	BlockTagRelation,
	BlockTreeInfo,
	SourceType,
	TagSimpleInfo,
} from '@/generated/data-contracts'
import { GlobalToken } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import { DataNode } from 'antd/es/tree'

export type BlockFlattenNestedTagInfo = BlockNestedTagInfo & {
	localName: string
	blockId: number
	parents: BlockSimpleInfo[]
}

export type FlattenedNestedTagsType = Record<number, BlockFlattenNestedTagInfo>

export const SELECTED_SEPARATOR: string = '~'
export const RELATION_TAG_SEPARATOR: string = '|'
export const BLOCK_ID_SHIFT: number = -1000000
export const VIRTUAL_RELATION_SHIFT: number = 1000000

const ALL_BLOCKS_ID = -1
const ORPHANS_ID = -2
const ALL_TAGS_ID = -3

const ALL_BLOCKS_NAME: string = '1. Дерево блоков'
const ORPHANS_NAME: string = '2. Нераспределенные теги'
const ALL_TAGS_NAME: string = '3. Все теги'
const FAKE_BLOCKS = [ALL_BLOCKS_NAME, ORPHANS_NAME, ALL_TAGS_NAME]

// Кодирует пару (blockId, tagId) в уникальное число
export const encodeBlockTagPair = (blockId: number, tagId: number): number => {
	// Для виртуальных блоков используем отрицательные значения
	if (blockId < 0) {
		return blockId * VIRTUAL_RELATION_SHIFT - tagId
	}
	return blockId * VIRTUAL_RELATION_SHIFT + tagId
}

// Декодирует число обратно в пару (blockId, tagId)
export const decodeBlockTagPair = (value: number): { blockId: number; tagId: number } => {
	if (value < 0) {
		const blockId = Math.ceil(value / VIRTUAL_RELATION_SHIFT)
		const tagId = -(value % VIRTUAL_RELATION_SHIFT)
		return { blockId, tagId }
	}
	return {
		blockId: Math.floor(value / VIRTUAL_RELATION_SHIFT),
		tagId: value % VIRTUAL_RELATION_SHIFT,
	}
}

export const convertToTreeSelectNodes = (
	blockTree: BlockTreeInfo[] | null | undefined,
	parentPath: string[] = [],
	token: GlobalToken,
	manual: boolean = false,
): DataNode[] => {
	if (!blockTree) return []

	return blockTree
		.map((block) => {
			const currentPath = [...parentPath]
			if (!FAKE_BLOCKS.includes(block.name)) currentPath.push(block.name)
			const fullTitle = currentPath.join('.')

			return {
				title: (
					<>
						<BlockIcon /> {block.name}
					</>
				),
				key: BLOCK_ID_SHIFT - block.id,
				value: BLOCK_ID_SHIFT - block.id,
				fullTitle,
				selectable: false,
				data: block,
				children: [
					...block.tags.map((tag) => {
						const value = encodeBlockTagPair(block.id, tag.id)
						return {
							title: (
								<div style={{ display: 'flex' }}>
									<TagIcon type={tag.sourceType} />
									&ensp;{tag.localName}
									&emsp;
									<pre style={{ display: 'inline-block', color: token.colorTextDisabled, margin: 0 }}>
										{block.id > 0 && block.id < VIRTUAL_RELATION_SHIFT && <>{tag.name}&emsp;</>}#{tag.id}
										{tag.resolution > 0 && <>&emsp;{getTagResolutionName(tag.resolution)}</>}
									</pre>
								</div>
							),
							key: value,
							value: value,
							fullTitle: `${fullTitle}.${tag.localName}`,
							data: { ...tag, blockId: block.id },
							disabled: manual && tag.sourceType !== SourceType.Manual,
						}
					}),
					...convertToTreeSelectNodes(block.children, currentPath, token, manual),
				],
			}
		})
		.sort((a, b) => a.fullTitle!.localeCompare(b.fullTitle!))
}

export const filterTreeNode = (inputValue: string, treeNode: DefaultOptionType): boolean => {
	const searchText = inputValue.toLowerCase()
	const node = treeNode

	if (node.fullTitle?.toLowerCase().includes(searchText)) return true

	if (node.data) {
		const data = node.data

		if ('fullName' in data) {
			const block = data as BlockTreeInfo
			if (block.name?.toLowerCase().includes(searchText) || block.fullName?.toLowerCase().includes(searchText)) {
				return true
			}
		} else if ('relationId' in data) {
			const tag = data as BlockNestedTagInfo
			if (
				tag.name?.toLowerCase().includes(searchText) ||
				tag.localName?.toLowerCase().includes(searchText) ||
				String(tag.id) == searchText ||
				tag.guid?.toLowerCase() == searchText
			) {
				return true
			}
		}
	}

	return false
}

export const createFullTree = ([blocksTree, allTags]: [blocksTree: BlockTreeInfo[], allTags: TagSimpleInfo[]]) => {
	const allTagIds = new Set<number>()
	const collectTagIds = (blocks: BlockTreeInfo[]) => {
		blocks.forEach((block) => {
			block.tags.forEach((tag) => allTagIds.add(tag.id))
			if (block.children) collectTagIds(block.children)
		})
	}
	collectTagIds(blocksTree)

	const orphanTags = allTags
		.filter((tag) => !allTagIds.has(tag.id))
		.map((tag) => ({
			...tag,
			relationId: -tag.id,
			relationType: BlockTagRelation.Static,
			localName: tag.name,
			sourceId: 0,
		}))

	const allBlocksBlock: BlockTreeInfo = {
		id: ALL_BLOCKS_ID,
		guid: 'virtual',
		name: ALL_BLOCKS_NAME,
		fullName: ALL_BLOCKS_NAME,
		tags: [],
		children: blocksTree,
		accessRule: {
			ruleId: 0,
			access: AccessType.Manager,
		},
	}

	const orphanTagsBlock: BlockTreeInfo = {
		id: ORPHANS_ID,
		guid: 'virtual',
		name: ORPHANS_NAME,
		fullName: ORPHANS_NAME,
		tags: orphanTags,
		children: [],
		accessRule: {
			ruleId: 0,
			access: AccessType.Manager,
		},
	}

	const allTagsBlock: BlockTreeInfo = {
		id: ALL_TAGS_ID,
		guid: 'all-tags',
		name: ALL_TAGS_NAME,
		fullName: ALL_TAGS_NAME,
		tags: allTags.map((tag) => ({
			...tag,
			relationId: VIRTUAL_RELATION_SHIFT + tag.id,
			relationType: BlockTagRelation.Static,
			localName: tag.name,
			sourceId: 0,
		})),
		children: [],
		accessRule: {
			ruleId: 0,
			access: AccessType.Manager,
		},
	}

	return [allBlocksBlock, orphanTagsBlock, allTagsBlock]
}
