import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import getTagResolutionName from '@/functions/getTagResolutionName'
import { RELATION_TAG_SEPARATOR } from '@/functions/urlParams'
import {
	AccessType,
	BlockNestedTagInfo,
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
	//parents: BlockSimpleInfo[]
}

export type FlattenedNestedTagsType = Record<string, BlockFlattenNestedTagInfo>

export type TagMappingType = Record<string, BlockFlattenNestedTagInfo>

const ALL_BLOCKS_ID = -1
const ORPHANS_ID = -2
const ALL_TAGS_ID = -3

const ALL_BLOCKS_NAME: string = '1.Дерево блоков'
const ORPHANS_NAME: string = '2.Нераспределенные теги'
const ALL_TAGS_NAME: string = '3.Все теги'

export const encodeBlockTagPair = (blockId: number, tagId: number) => `${blockId}${RELATION_TAG_SEPARATOR}${tagId}`

export const decodeBlockTagPair = (raw: string) => {
	const parts = raw.split(RELATION_TAG_SEPARATOR).map(Number)
	return { blockId: parts[0], tagId: parts[1] }
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
			currentPath.push(block.name)
			const fullTitle = currentPath.join('.')
			const blockValue = encodeBlockTagPair(block.id, 0)

			return {
				title: (
					<>
						<BlockIcon /> {block.name}
					</>
				),
				key: blockValue,
				value: blockValue,
				fullTitle,
				selectable: false,
				data: block,
				children: [
					...block.tags.map((tag) => {
						const tagId = tag.tag?.id ?? tag.tagId ?? 0
						const tagValue = encodeBlockTagPair(block.id, tagId)
						return {
							title: (
								<div style={{ display: 'flex' }}>
									<TagIcon type={tag.tag?.sourceType ?? SourceType.Unset} />
									&ensp;{tag.localName ?? tag.tag?.name ?? ''}
									&emsp;
									<pre style={{ display: 'inline-block', color: token.colorTextDisabled, margin: 0 }}>
										{block.id > 0 && tag.tag?.name && <>{tag.tag.name}&emsp;</>}#{tagId}
										{tag.tag?.resolution && tag.tag.resolution > 0 && <>&emsp;{getTagResolutionName(tag.tag.resolution)}</>}
									</pre>
								</div>
							),
							key: tagValue,
							value: tagValue,
							fullTitle: `${fullTitle}.${tag.localName ?? tag.tag?.name ?? ''}`,
							data: { ...tag, blockId: block.id },
							disabled: manual && tag.tag?.sourceType !== SourceType.Manual,
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

		if ('children' in data && 'tags' in data) {
			const block = data as BlockTreeInfo
			if (block.name?.toLowerCase().includes(searchText)) {
				return true
			}
		} else if ('relationType' in data) {
			const tag = data as BlockNestedTagInfo
			const tagId = tag.tag?.id ?? tag.tagId
			if (
				tag.tag?.name?.toLowerCase().includes(searchText) ||
				tag.localName?.toLowerCase().includes(searchText) ||
				(tagId && String(tagId) == searchText) ||
				tag.tag?.guid?.toLowerCase() == searchText
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
			block.tags.forEach((tag) => {
				const tagId = tag.tag?.id ?? tag.tagId
				if (tagId) allTagIds.add(tagId)
			})
			if (block.children) collectTagIds(block.children)
		})
	}
	collectTagIds(blocksTree)

	const orphanTags: BlockNestedTagInfo[] = allTags
		.filter((tag) => !allTagIds.has(tag.id))
		.map((tag) => ({
			relationType: BlockTagRelation.Static,
			localName: tag.name,
			tag: tag,
			tagId: tag.id,
			blockId: 0,
		}))

	const allBlocksBlock: BlockTreeInfo = {
		id: ALL_BLOCKS_ID,
		guid: 'virtual',
		name: ALL_BLOCKS_NAME,
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
		tags: allTags.map((tag) => ({
			relationType: BlockTagRelation.Static,
			localName: tag.name,
			tag: tag,
			tagId: tag.id,
			blockId: 0,
		})),
		children: [],
		accessRule: {
			ruleId: 0,
			access: AccessType.Manager,
		},
	}

	return [allBlocksBlock, orphanTagsBlock, allTagsBlock]
}
