import api from '@/api/swagger-api'
import {
	AccessType,
	BlockNestedTagInfo,
	BlockSimpleInfo,
	BlockTagRelation,
	BlockTreeInfo,
	TagInfo,
} from '@/api/swagger/data-contracts'
import BlockIcon from '@/app/components/icons/BlockIcon'
import TagIcon from '@/app/components/icons/TagIcon'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

interface QueryTreeSelectProps {
	onChange: (value: number[], tagMapping: FlattenedNestedTagsType) => void
}

const SELECTED_SEPARATOR: string = '~'
const RELATION_TAG_SEPARATOR: string = '.'
const BLOCK_ID_SHIFT: number = -1000000
const VIRTUAL_RELATION_SHIFT: number = 1000000

// Функция для преобразования BlockTreeInfo[] в древовидную структуру
function convertToTreeSelectNodes(
	blockTree: BlockTreeInfo[] | null | undefined,
	parentPath: string[] = [],
): DefaultOptionType[] {
	if (!blockTree) return []

	return blockTree
		.map((block) => {
			const currentPath = [...parentPath, block.name]
			const fullTitle = currentPath.join('.')

			return {
				title: (
					<>
						<BlockIcon /> {block.name}
					</>
				),
				value: BLOCK_ID_SHIFT - 3 - block.id, // Отрицательные значения для блоков (и поправка на 3 из-за тех, которые мы создаем сами)
				fullTitle,
				selectable: false, // Блоки не выбираются
				data: block,
				children: [
					...block.tags.map((tag) => ({
						title: (
							<>
								<TagIcon type={tag.sourceType} /> {tag.localName} <TagFrequencyEl frequency={tag.frequency} />
							</>
						),
						value: tag.relationId, // Используем relationId как идентификатор
						fullTitle: `${fullTitle}.${tag.localName}`,
						data: tag,
					})),
					...convertToTreeSelectNodes(block.children, currentPath),
				],
			}
		})
		.sort((a, b) => a.fullTitle!.localeCompare(b.fullTitle!))
}

// Функция для создания маппинга тегов
const flattenNestedTags = (
	blockTree: BlockTreeInfo[] | null | undefined,
	parentNames: BlockSimpleInfo[] = [],
): FlattenedNestedTagsType => {
	let mapping: FlattenedNestedTagsType = {}
	if (!blockTree) return mapping

	blockTree.forEach((block) => {
		const currentParents = [...parentNames, { ...block }]
		block.tags.forEach((tag) => {
			mapping[tag.relationId] = {
				...tag,
				parents: currentParents,
			}
		})
		const childrenMapping = flattenNestedTags(block.children, currentParents)
		mapping = { ...mapping, ...childrenMapping }
	})

	return mapping
}

const QueryTreeSelect: React.FC<QueryTreeSelectProps> = ({ onChange }) => {
	const [searchParams, setSearchParams] = useSearchParams()
	const [checkedRelations, setCheckedRelations] = useState<number[]>([])
	const [treeData, setTreeData] = useState<DefaultOptionType[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>({})
	const [searchValue, setSearchValue] = useState<string>('')
	const [loading, setLoading] = useState<boolean>(false)

	// Инициализация выбранных значений из query-параметров
	const initialSelections = useMemo(() => {
		const param = searchParams.get('tags')
		if (!param) return []

		const parsed = param.split(SELECTED_SEPARATOR).map((pair) => {
			const [tagId, relationId] = pair.split(RELATION_TAG_SEPARATOR).map(Number)
			return { tagId, relationId }
		})

		return parsed.filter((sel) => !isNaN(sel.tagId) && !isNaN(sel.relationId))
	}, [searchParams])

	// Загрузка данных и формирование дерева
	useEffect(() => {
		setLoading(false)
		Promise.all([
			api
				.blocksReadAsTree()
				.then((res) => res.data)
				.catch(() => [] as BlockTreeInfo[]),
			api
				.tagsReadAll()
				.then((res) => res.data)
				.catch(() => [] as TagInfo[]),
		]).then(([blocksTree, allTags]) => {
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
				name: '1. Дерево блоков',
				fullName: '1. Дерево блоков',
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
				name: '2. Нераспределенные теги',
				fullName: '2. Нераспределенные теги',
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
				name: '3. Все теги',
				fullName: '3. Все теги',
				tags: allTags.map((tag) => ({
					...tag,
					relationId: VIRTUAL_RELATION_SHIFT - tag.id, // Виртуальные связи
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

			// Формируем маппинг relationId -> tagId
			const relationMap: Record<number, number> = {}
			const buildRelationMap = (blocks: BlockTreeInfo[]) => {
				blocks.forEach((block) => {
					block.tags.forEach((tag) => {
						relationMap[tag.relationId] = tag.id
					})
					if (block.children) buildRelationMap(block.children)
				})
			}
			buildRelationMap(fullTree)

			setTagMapping(flattenNestedTags(fullTree))
			setTreeData(convertToTreeSelectNodes(fullTree))
			setLoading(true)
		})
	}, [])

	// Восстановление выбранных значений при загрузке
	useEffect(() => {
		if (loading && treeData.length > 0) {
			// Восстанавливаем relationId из начальных значений
			const relationsToSelect = initialSelections.map((sel) => sel.relationId)
			setCheckedRelations(relationsToSelect)

			// ВЫЗОВ ONCHANGE ПРИ ИНИЦИАЛИЗАЦИИ
			if (relationsToSelect.length > 0) {
				// Создаем маппинг для передачи
				const initialMapping: FlattenedNestedTagsType = {}
				relationsToSelect.forEach((relId) => {
					if (tagMapping[relId]) {
						initialMapping[relId] = tagMapping[relId]
					}
				})

				onChange(relationsToSelect, initialMapping)
			}
		}
	}, [loading, treeData, initialSelections, onChange, tagMapping])

	// Обработчик изменения выбранных значений
	const handleTagChange = useCallback(
		(values: number[]) => {
			// Фильтруем только теги (исключаем блоки)
			const tagValues = values.filter((val) => val > BLOCK_ID_SHIFT) // Исключаем блоки, оставляем фейковые связи (отрицательные)

			if (!isArraysDifferent(checkedRelations, tagValues)) return

			setCheckedRelations(tagValues)

			// Создаем пары [tagId, relationId] для сохранения в URL
			const selections: string[] = []
			tagValues.forEach((relationId) => {
				const mapping = tagMapping[relationId]
				if (mapping) selections.push(`${mapping.id}${RELATION_TAG_SEPARATOR}${relationId}`)
				return
			})

			// Вызываем внешний обработчик с relationId
			onChange(tagValues, tagMapping) // Исправлено: передаем relationId

			// Обновляем query-параметры
			searchParams.set('tags', selections.join(SELECTED_SEPARATOR))
			setSearchParams(searchParams, { replace: true })
		},
		[onChange, searchParams, setSearchParams, tagMapping, checkedRelations],
	)

	// Функция фильтрации узлов
	const filterTreeNode = (inputValue: string, treeNode: DefaultOptionType): boolean => {
		const searchText = inputValue.toLowerCase()
		const node = treeNode

		// Поиск по полному пути
		if (node.fullTitle?.toLowerCase().includes(searchText)) return true

		// Поиск по данным узла
		if (node.data) {
			const data = node.data

			// Поиск по ID
			if (data.id?.toString().includes(searchText)) return true

			// Поиск по GUID
			if (data.guid?.toLowerCase().includes(searchText)) return true

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
					tag.relationId?.toString().includes(searchText)
				) {
					return true
				}
			}
		}

		return false
	}

	// Подсчет уникальных выбранных тегов
	const countSelectedTags = useCallback(() => {
		return checkedRelations.length
	}, [checkedRelations])

	return (
		<TreeSelect
			treeData={treeData}
			treeCheckable
			allowClear
			showCheckedStrategy={TreeSelect.SHOW_ALL}
			value={checkedRelations}
			onChange={handleTagChange}
			placeholder='Выберите теги'
			style={{ width: '100%' }}
			maxTagCount={0}
			maxTagPlaceholder={() => `Выбрано тегов: ${countSelectedTags()}`}
			filterTreeNode={filterTreeNode}
			searchValue={searchValue}
			onSearch={setSearchValue}
		/>
	)
}

export default QueryTreeSelect
