import api from '@/api/swagger-api'
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
import isArraysDifferent from '@/functions/isArraysDifferent'
import { TreeSelect } from 'antd'
import { DefaultOptionType } from 'antd/es/cascader'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

interface QueryTreeSelectProps {
	onChange: (value: number[], tagMapping: FlattenedNestedTagsType, checkedNodes: DefaultOptionType[]) => void
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

const convertToTreeSelectNodes = (blockTree: BlockTreeInfo[] | null | undefined): DefaultOptionType[] => {
	if (!blockTree) return []
	return blockTree
		.map((block) => ({
			title: (
				<>
					<BlockIcon />
					&ensp;{block.name}
				</>
			),
			fullTitle: block.name,
			key: 0 - block.id,
			value: 0 - block.id,
			children: [
				...block.tags
					.map((tag) => ({
						title: (
							<>
								<TagIcon type={tag.sourceType} />
								&ensp;
								{tag.localName}&ensp;
								<TagFrequencyEl frequency={tag.frequency} />
							</>
						),
						fullTitle: tag.localName,
						key: tag.id,
						value: tag.id,
					}))
					.sort((a, b) => a.fullTitle.localeCompare(b.fullTitle)),
				...convertToTreeSelectNodes(block.children),
			],
		}))
		.sort((a, b) => a.fullTitle.localeCompare(b.fullTitle))
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
	const [loading, setLoading] = useState<boolean>(false)

	useEffect(() => {
		setLoading(false)
		let blocksTree: BlockTreeInfo[]
		let tags: TagSimpleInfo[]
		Promise.all([
			api
				.blocksReadAsTree()
				.then((res) => (blocksTree = res.data))
				.catch(() => (blocksTree = [])),
			api
				.tagsReadAll()
				.then((res) => (tags = res.data))
				.catch(() => (tags = [])),
		]).then(() => {
			const mappingFromBlocks = flattenNestedTags(blocksTree)
			tags = tags.filter((tag) => !mappingFromBlocks[tag.id])

			const fakeBlock: BlockTreeInfo = {
				id: 0,
				guid: 'fake',
				name: 'Нераспределенные теги',
				fullName: 'Нераспределенные теги',
				tags: tags
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
					access: AccessType.Manager,
				},
			}
			blocksTree.push(fakeBlock)

			setTagMapping({
				...mappingFromBlocks,
				...flattenNestedTags([fakeBlock]),
			})
			setTreeData(convertToTreeSelectNodes(blocksTree))
			setLoading(true)
		})
	}, [])

	const handleTagChange = useCallback(
		(value: number[], _: React.ReactNode[], extra: DefaultOptionType) => {
			const currentTags = value.filter((x) => x > 0)
			if (!isArraysDifferent(checkedTags, currentTags)) return

			setCheckedTags(value)
			onChange(value, tagMapping, extra?.checkedNodes)
			searchParams.set('tags', value.filter((x) => x > 0).join('|'))
			setSearchParams(searchParams, { replace: true })
		},
		[onChange, searchParams, setSearchParams, tagMapping, checkedTags],
	)

	const filterTreeNode = (inputValue: string, treeNode: DefaultOptionType) => {
		const search = inputValue.toLowerCase()
		const mapping = tagMapping[Number(treeNode.value)]
		const node = treeNode as DefaultOptionType
		if (!node || !mapping) return false

		const localName = node.fullTitle || ''
		const globalName = mapping.name
		const guid = mapping.guid
		const id = String(node.value)

		return (
			[localName, globalName, guid, id].filter((x) => {
				return x.toLowerCase().includes(search)
			}).length > 0
		)
	}

	useEffect(() => {
		if (loading) {
			setLoading(false)
			onChange(checkedTags, tagMapping, [])
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [loading])

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
			maxTagPlaceholder={(omittedValues) => `Выбрано тегов: ${omittedValues.filter((x) => Number(x.value) > 0).length}`}
			filterTreeNode={filterTreeNode}
			searchValue={searchValue}
			onSearch={setSearchValue}
		/>
	)
}

export default QueryTreeSelect
