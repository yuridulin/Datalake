import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import getTagResolutionName from '@/functions/getTagResolutionName'
import {
	AccessType,
	BlockNestedTagInfo,
	BlockTagRelation,
	BlockTreeInfo,
	TagSimpleInfo,
} from '@/generated/data-contracts'
import { GlobalToken } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import { DataNode } from 'antd/es/tree'

export const SELECTED_SEPARATOR: string = '~'
export const RELATION_TAG_SEPARATOR: string = '.'
export const BLOCK_ID_SHIFT: number = -1000000
export const VIRTUAL_RELATION_SHIFT: number = 1000000

const ALL_BLOCKS_NAME: string = '1. Дерево блоков'
const ORPHANS_NAME: string = '2. Нераспределенные теги'
const ALL_TAGS_NAME: string = '3. Все теги'
const FAKE_BLOCKS = [ALL_BLOCKS_NAME, ORPHANS_NAME, ALL_TAGS_NAME]

// Функция для преобразования BlockTreeInfo[] в древовидную структуру
export const convertToTreeSelectNodes = (
	blockTree: BlockTreeInfo[] | null | undefined,
	parentPath: string[] = [],
	token: GlobalToken,
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
				key: BLOCK_ID_SHIFT - 3 - block.id, // Отрицательные значения для блоков (и поправка на 3 из-за тех, которые мы создаем сами)
				value: BLOCK_ID_SHIFT - 3 - block.id,
				fullTitle,
				selectable: false, // Блоки не выбираются
				data: block,
				children: [
					...block.tags.map((tag) => ({
						title: (
							<>
								<TagIcon type={tag.sourceType} />
								&ensp;{tag.localName}
								&emsp;
								<pre style={{ display: 'inline-block', color: token.colorTextDisabled }}>
									{tag.relationId > 0 && tag.relationId < VIRTUAL_RELATION_SHIFT && <>{tag.name}&emsp;</>}#{tag.id}
									{tag.resolution > 0 && <>&emsp;{getTagResolutionName(tag.resolution)}</>}
								</pre>
							</>
						),
						key: tag.relationId, // Используем relationId как идентификатор
						value: tag.relationId, // Используем relationId как идентификатор
						fullTitle: `${fullTitle}.${tag.localName}`,
						data: tag,
					})),
					...convertToTreeSelectNodes(block.children, currentPath, token),
				],
			}
		})
		.sort((a, b) => a.fullTitle!.localeCompare(b.fullTitle!))
}

// Функция фильтрации узлов
export const filterTreeNode = (inputValue: string, treeNode: DefaultOptionType): boolean => {
	const searchText = inputValue.toLowerCase()
	const node = treeNode

	// Поиск по полному пути
	if (node.fullTitle?.toLowerCase().includes(searchText)) return true

	// Поиск по данным узла
	if (node.data) {
		const data = node.data

		// Для блоков
		if ('fullName' in data) {
			const block = data as BlockTreeInfo
			if (block.name?.toLowerCase().includes(searchText) || block.fullName?.toLowerCase().includes(searchText)) {
				return true
			}
		}
		// Для тегов
		else if ('relationId' in data) {
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
	// Собираем ID всех тегов в дереве
	const allTagIds = new Set<number>()
	const collectTagIds = (blocks: BlockTreeInfo[]) => {
		blocks.forEach((block) => {
			block.tags.forEach((tag) => allTagIds.add(tag.id))
			if (block.children) collectTagIds(block.children)
		})
	}
	collectTagIds(blocksTree)

	// Фильтруем нераспределенные теги
	const orphanTags = allTags
		.filter((tag) => !allTagIds.has(tag.id))
		.map((tag, index) => ({
			...tag,
			relationId: -(index + 1), // Уникальные отрицательные ID для связей
			relationType: BlockTagRelation.Static,
			localName: tag.name,
			sourceId: 0,
		}))

	// Создаем общий контейнер для дерева блоков
	const allBlocksBlock: BlockTreeInfo = {
		id: -1,
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

	// Создаем виртуальный блок для нераспределенных тегов
	const orphanTagsBlock: BlockTreeInfo = {
		id: -2,
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

	// Создаем виртуальный блок для всех тегов
	const allTagsBlock: BlockTreeInfo = {
		id: -3, // Уникальный ID
		guid: 'all-tags',
		name: ALL_TAGS_NAME,
		fullName: ALL_TAGS_NAME,
		tags: allTags.map((tag) => ({
			...tag,
			relationId: VIRTUAL_RELATION_SHIFT + tag.id, // Виртуальные связи
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

	// Создаем полное дерево
	const fullTree = [allBlocksBlock, orphanTagsBlock, allTagsBlock]
	return fullTree
}
