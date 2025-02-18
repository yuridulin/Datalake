import api from '@/api/swagger-api'
import { BlockSimpleInfo, BlockTreeInfo } from '@/api/swagger/data-contracts'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

interface QueryTreeSelectProps {
	onChange: (
		value: number[],
		tagMapping: FlattenedNestedTagsType,
		checkedNodes: DefaultOptionType[],
	) => void
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
		const childrenMapping = flattenNestedTags(
			block.children,
			currentParents,
		)
		mapping = { ...mapping, ...childrenMapping }
	})

	return mapping
}

const convertToTreeSelectNodes = (
	blockTree: BlockTreeInfo[],
): DefaultOptionType[] => {
	return blockTree.map((block) => ({
		title: block.name,
		key: 0 - block.id,
		value: 0 - block.id,
		children: [
			...block.tags.map((tag) => ({
				title: tag.localName,
				key: tag.id,
				value: tag.id,
			})),
			...convertToTreeSelectNodes(block.children),
		],
	}))
}

const QueryTreeSelect: React.FC<QueryTreeSelectProps> = ({ onChange }) => {
	const [searchParams, setSearchParams] = useSearchParams()
	const initialTags = useMemo(
		() =>
			searchParams
				.get('tags')
				?.split('|')
				.map(Number)
				.filter((tag) => tag > 0) || [],
		[searchParams],
	)
	const [checkedTags, setCheckedTags] = useState<number[]>(initialTags)
	const [treeData, setTreeData] = useState<DefaultOptionType[]>([])
	const [tagMapping, setTagMapping] = useState<FlattenedNestedTagsType>([])
	const [searchValue, setSearchValue] = useState<string>('')

	useEffect(() => {
		api.blocksReadAsTree()
			.then((res) => {
				const mapping = flattenNestedTags(res.data)
				setTagMapping(mapping)
				const data = convertToTreeSelectNodes(res.data)
				setTreeData(data)
			})
			.catch(() => {
				setTreeData([])
				setTagMapping({})
			})
	}, [])

	const handleTagChange = useCallback(
		(value: number[], _: React.ReactNode[], extra: DefaultOptionType) => {
			setCheckedTags(value)
			onChange(value, tagMapping, extra?.checkedNodes)
			searchParams.set('tags', value.filter((x) => x > 0).join('|'))
			setSearchParams(searchParams, { replace: true })
		},
		[onChange, searchParams, setSearchParams, tagMapping],
	)

	const filterTreeNode = (
		inputValue: string,
		treeNode: DefaultOptionType,
	) => {
		const search = inputValue.toLowerCase()
		const mapping = tagMapping[Number(treeNode.value)]
		const node = treeNode as DefaultOptionType
		if (!node || !mapping) return false

		const localName = node.title || ''
		const globalName = mapping.name
		const guid = mapping.guid
		const id = String(node.value)

		return (
			[localName, globalName, guid, id].filter((x) =>
				x.toLowerCase().includes(search),
			).length > 0
		)
	}

	useEffect(() => {
		if (tagMapping && checkedTags) {
			onChange(checkedTags, tagMapping, [])
		}
	}, [tagMapping, checkedTags, onChange])

	return (
		<TreeSelect
			treeData={treeData}
			treeCheckable
			allowClear
			showCheckedStrategy={TreeSelect.SHOW_ALL}
			value={checkedTags}
			onChange={handleTagChange}
			placeholder='Выберите теги'
			style={{ width: '100%' }}
			maxTagCount={0}
			maxTagPlaceholder={(omittedValues) =>
				`Выбрано тегов: ${omittedValues.length}`
			}
			filterTreeNode={filterTreeNode}
			searchValue={searchValue}
			onSearch={setSearchValue}
		/>
	)
}

export default QueryTreeSelect
