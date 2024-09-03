import { Button, Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useCallback, useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../../api/models/customSource'
import getDictFromValuesResponseArray from '../../../api/models/getDictFromValuesResponseArray'
import api from '../../../api/swagger-api'
import {
	SourceType,
	TagInfo,
	TagType,
} from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import SourceEl from '../../components/SourceEl'
import TagTypeEl from '../../components/TagTypeEl'
import TagValueEl from '../../components/TagValueEl'

interface TagsTableProps {
	tags: TagInfo[]
	hideSource?: boolean
	hideValue?: boolean
	hideType?: boolean
}

export default function TagsTable({
	tags,
	hideSource = false,
	hideValue = false,
	hideType = false,
}: TagsTableProps) {
	const [viewingTags, setViewingTags] = useState(tags)
	const [search, setSearch] = useState('')
	const [viewingTagsValues, setViewingTagsValues] = useState(
		{} as { [key: string]: any },
	)

	const loadValues = useCallback(() => {
		api.valuesGet([
			{ requestKey: 'tags-table', tags: viewingTags.map((x) => x.guid) },
		])
			.then(
				(res) =>
					res.status === 200 &&
					setViewingTagsValues(
						getDictFromValuesResponseArray(res.data),
					),
			)
			.catch(() =>
				setViewingTagsValues(
					Object.fromEntries(
						Object.keys(viewingTags).map((prop) => [prop, null]),
					),
				),
			)
	}, [viewingTags])

	const prepareValues = useCallback(() => {
		const values = viewingTags
			.map((x) => ({ [x.guid ?? 0]: '' }))
			.reduce((next, current) => ({ ...next, ...current }), {})
		setViewingTagsValues(values)
		loadValues()
	}, [viewingTags, loadValues])

	const doSearch = useCallback(() => {
		setViewingTags(
			search.length > 0
				? tags.filter(
						(x) =>
							!!x.name &&
							x.name.toLowerCase().includes(search.toLowerCase()),
				  )
				: tags,
		)
	}, [search, tags])

	useEffect(doSearch, [doSearch, search, tags])
	useEffect(prepareValues, [prepareValues])
	useInterval(loadValues, 5000)

	return (
		<Table size='middle' dataSource={viewingTags} showSorterTooltip={false}>
			<Column
				title={
					<Input
						placeholder='Поиск по имени тега'
						value={search}
						onClick={(e) => {
							e.preventDefault()
							e.stopPropagation()
						}}
						onChange={(e) => {
							setSearch(e.target.value)
						}}
					/>
				}
				dataIndex='Name'
				key='Id'
				defaultSortOrder='ascend'
				sorter={(a: TagInfo, b: TagInfo) =>
					(a.name ?? '').localeCompare(b.name ?? '')
				}
				render={(_, tag) => (
					<NavLink to={`/tags/${tag.guid}`}>
						<Button>{tag.name}</Button>
					</NavLink>
				)}
			/>
			{!hideSource && (
				<Column
					title='Источник'
					dataIndex='SourceId'
					key='Id'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						(a.sourceName ?? String(a.sourceId)).localeCompare(
							b.sourceName ?? String(b.sourceId),
						)
					}
					render={(_, record) => (
						<SourceEl
							id={record.sourceId ?? 0}
							name={record.sourceName ?? '?'}
						/>
					)}
				/>
			)}
			{!hideType && (
				<Column
					title='Тип'
					dataIndex='Type'
					key='Id'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						Number(a.type) - Number(b.type)
					}
					render={(_, record) => (
						<TagTypeEl tagType={record.type ?? TagType.String} />
					)}
				/>
			)}
			{!hideValue && (
				<Column
					title='Значение'
					dataIndex='Value'
					key='Id'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						String(viewingTagsValues[a.guid ?? 0]).localeCompare(
							String(viewingTagsValues[b.guid ?? 0]),
						)
					}
					render={(_, record: TagInfo) => (
						<TagValueEl
							value={viewingTagsValues[record.guid ?? 0]}
							allowEdit={
								record.sourceType === SourceType.Custom &&
								record.sourceId === CustomSource.Manual
							}
							guid={record.guid}
						/>
					)}
				/>
			)}
			<Column
				title='Описание'
				dataIndex='Description'
				key='Id'
				defaultSortOrder='ascend'
				sorter={(a: TagInfo, b: TagInfo) =>
					(a.description ?? '').localeCompare(b.description ?? '')
				}
			/>
		</Table>
	)
}
