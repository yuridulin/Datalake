import { Button, Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import getDictFromValuesResponseArray from '../../../api/models/getDictFromValuesResponseArray'
import api from '../../../api/swagger-api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
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
	const [data, setData] = useState([] as TagInfo[])
	const [search, setSearch] = useState('')
	const [values, setValues] = useState({} as { [key: string]: any })

	function prepareValues() {
		setValues(
			tags
				.map((x) => ({ [x.guid ?? 0]: '' }))
				.reduce((next, current) => ({ ...next, ...current }), {}),
		)
	}

	function loadValues() {
		api.valuesGet([
			{ requestKey: 'tags-table', tags: tags.map((x) => x.guid) },
		]).then(
			(res) =>
				res.status === 200 &&
				setValues(getDictFromValuesResponseArray(res.data)),
		)
	}

	function doSearch() {
		setData(
			tags.filter(
				(x) =>
					!!x.name &&
					x.name.toLowerCase().includes(search.toLowerCase()),
			),
		)
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(doSearch, [search])
	useEffect(prepareValues, [tags])
	useInterval(loadValues, 5000)

	return tags.length > 0 ? (
		<Table size='middle' dataSource={data} showSorterTooltip={false}>
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
				key='Name'
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
			{!hideType && (
				<Column
					title='Тип'
					dataIndex='Type'
					key='Type'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						Number(a.type) - Number(b.type)
					}
					render={(_, record) => (
						<TagTypeEl tagType={record.type ?? TagType.String} />
					)}
				/>
			)}
			{!hideSource && (
				<Column
					title='Источник'
					dataIndex='SourceId'
					key='SourceId'
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
			<Column
				title='Описание'
				dataIndex='Description'
				key='Description'
				defaultSortOrder='ascend'
				sorter={(a: TagInfo, b: TagInfo) =>
					(a.description ?? '').localeCompare(b.description ?? '')
				}
			/>
			{!hideValue && (
				<Column
					title='Значение'
					dataIndex='Value'
					key='Value'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						String(values[a.guid ?? 0]).localeCompare(
							String(values[b.guid ?? 0]),
						)
					}
					render={(_, record: TagInfo) => (
						<TagValueEl value={values[record.guid ?? 0]} />
					)}
				/>
			)}
		</Table>
	) : (
		<></>
	)
}
