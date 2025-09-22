import {
	convertToTreeSelectNodes,
	createFullTree,
	encodeBlockTagPair,
	filterTreeNode,
	FlattenedNestedTagsType,
	TagMappingType,
} from '@/app/components/tagTreeSelect/treeSelectShared'
import isArraysDifferent from '@/functions/isArraysDifferent'
import { RELATION_TAG_SEPARATOR, SELECTED_SEPARATOR, URL_PARAMS } from '@/functions/urlParams'
import { BlockSimpleInfo, BlockTreeInfo, TagSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { theme, TreeSelect } from 'antd'
import { DataNode } from 'antd/es/tree'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

interface QueryTreeSelectProps {
	onChange: (value: string[], tagMapping: TagMappingType) => void
	manualOnly?: true
}

const flattenNestedTags = (
	blockTree: BlockTreeInfo[] | null | undefined,
	parentNames: BlockSimpleInfo[] = [],
): FlattenedNestedTagsType => {
	let mapping: FlattenedNestedTagsType = {}
	if (!blockTree) return mapping

	blockTree.forEach((block) => {
		const currentParents = [...parentNames, { ...block }]
		block.tags.forEach((tag) => {
			const value = encodeBlockTagPair(block.id, tag.id)
			mapping[value] = {
				...tag,
				blockId: block.id,
			}
		})
		const childrenMapping = flattenNestedTags(block.children, currentParents)
		mapping = { ...mapping, ...childrenMapping }
	})

	return mapping
}

const QueryTreeSelect: React.FC<QueryTreeSelectProps> = ({ onChange, manualOnly = false }) => {
	const store = useAppStore()
	const [searchParams, setSearchParams] = useSearchParams()
	const [checkedValues, setCheckedValues] = useState<string[]>([])
	const [treeData, setTreeData] = useState<DataNode[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>({})
	const [searchValue, setSearchValue] = useState<string>('')
	const [loading, setLoading] = useState<boolean>(false)
	const { token } = theme.useToken()

	const initialSelections = useMemo(() => {
		const param = searchParams.get(URL_PARAMS.TAGS)
		if (!param) return []
		return param.split('~')
	}, [searchParams])

	useEffect(() => {
		setLoading(false)
		Promise.all([
			store.api
				.blocksGetTree()
				.then((res) => res.data)
				.catch(() => [] as BlockTreeInfo[]),
			store.api
				.tagsGetAll()
				.then((res) => res.data)
				.catch(() => [] as TagSimpleInfo[]),
		]).then((data) => {
			const fullTree = createFullTree(data)
			setTagMapping(flattenNestedTags(fullTree))
			setTreeData(convertToTreeSelectNodes(fullTree, undefined, token, manualOnly))
			setLoading(true)
		})
	}, [token, store.api, manualOnly])

	useEffect(() => {
		if (!loading) return
		if (!treeData.length) return
		if (!initialSelections.length) return

		setCheckedValues(initialSelections)
		const initialMapping: FlattenedNestedTagsType = {}
		initialSelections.forEach((value) => {
			if (tagMapping[value]) {
				initialMapping[value] = tagMapping[value]
			}
		})
		onChange(initialSelections, initialMapping)
	}, [loading, treeData, initialSelections, onChange, tagMapping])

	const handleTagChange = useCallback(
		(values: string[]) => {
			if (!isArraysDifferent(checkedValues, values)) return

			const realValues = values.filter((x) => !x.includes(`${RELATION_TAG_SEPARATOR}0`))

			setCheckedValues(realValues)
			onChange(realValues, tagMapping)

			setSearchParams(
				(prev) => {
					prev.set(URL_PARAMS.TAGS, realValues.join(SELECTED_SEPARATOR))
					return prev
				},
				{ replace: true },
			)
		},
		[onChange, setSearchParams, tagMapping, checkedValues],
	)

	const countSelectedTags = useMemo(() => checkedValues.length, [checkedValues])

	return (
		<TreeSelect
			treeData={treeData}
			treeCheckable
			allowClear
			showCheckedStrategy={TreeSelect.SHOW_ALL}
			value={checkedValues}
			onChange={handleTagChange}
			placeholder='Выберите теги'
			style={{ width: '100%' }}
			styles={{ popup: { root: { maxHeight: '80vh', overflow: 'auto' } } }}
			listHeight={1000}
			virtual={true}
			maxTagCount={0}
			maxTagPlaceholder={() => `Выбрано тегов: ${countSelectedTags}`}
			filterTreeNode={filterTreeNode}
			searchValue={searchValue}
			onSearch={setSearchValue}
		/>
	)
}

export default QueryTreeSelect
