import api from '@/api/swagger-api'
import { BlockSimpleInfo, BlockTreeInfo, TagSimpleInfo } from '@/api/swagger/data-contracts'
import {
	BLOCK_ID_SHIFT,
	convertToTreeSelectNodes,
	createFullTree,
	filterTreeNode,
	RELATION_TAG_SEPARATOR,
	SELECTED_SEPARATOR,
} from '@/app/components/tagTreeSelect/treeSelectShared'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { theme, TreeSelect } from 'antd'
import { DataNode } from 'antd/es/tree'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

interface QueryTreeSelectProps {
	onChange: (value: number[], tagMapping: FlattenedNestedTagsType) => void
}

const TagsParam = 'T'

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
	const [treeData, setTreeData] = useState<DataNode[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>({})
	const [searchValue, setSearchValue] = useState<string>('')
	const [loading, setLoading] = useState<boolean>(false)
	const { token } = theme.useToken()

	// Инициализация выбранных значений из query-параметров
	const initialSelections = useMemo(() => {
		const param = searchParams.get(TagsParam)
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
				.blocksGetTree()
				.then((res) => res.data)
				.catch(() => [] as BlockTreeInfo[]),
			api
				.tagsGetAll()
				.then((res) => res.data)
				.catch(() => [] as TagSimpleInfo[]),
		]).then((data) => {
			// Создаем полное дерево
			const fullTree = createFullTree(data)

			setTagMapping(flattenNestedTags(fullTree))
			setTreeData(convertToTreeSelectNodes(fullTree, undefined, token))
			setLoading(true)
		})
	}, [token])

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
			onChange(tagValues, tagMapping)

			// Обновляем query-параметры
			searchParams.set(TagsParam, selections.join(SELECTED_SEPARATOR))
			setSearchParams(searchParams, { replace: true })
		},
		[onChange, searchParams, setSearchParams, tagMapping, checkedRelations],
	)

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
			dropdownStyle={{ maxHeight: '80vh', overflow: 'auto' }}
			listHeight={1000}
			virtual={true}
			maxTagCount={0}
			maxTagPlaceholder={() => `Выбрано тегов: ${countSelectedTags()}`}
			filterTreeNode={filterTreeNode}
			searchValue={searchValue}
			onSearch={setSearchValue}
		/>
	)
}

export default QueryTreeSelect
