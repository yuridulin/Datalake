import { Button, Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import SourceEl from '../../small/SourceEl'
import TagTypeEl from '../../small/TagTypeEl'
import TagValueEl from '../../small/TagValueEl'

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
	const [values, setValues] = useState({} as { [key: number]: any })

	function prepareValues() {
		setValues(
			tags
				.map((x) => ({ [x.id ?? 0]: '' }))
				.reduce((next, current) => ({ ...next, ...current }), {}),
		)
	}

	function loadValues() {
		api.valuesGet([{ tags: tags.map((x) => Number(x.id)) }]).then(
			(res) =>
				res.status === 200 &&
				setValues(
					res.data
						.map((x) => ({
							[x.id ?? 0]: !!x.values
								? x.values.length > 0
									? x.values[0].value ?? ''
									: ''
								: '',
						}))
						.reduce(
							(next, current) => ({ ...next, ...current }),
							{},
						),
				),
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
					<NavLink to={`/tags/${tag.id}`}>
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
						(
							a.sourceInfo?.name ?? String(a.sourceInfo?.id)
						).localeCompare(
							b.sourceInfo?.name ?? String(b.sourceInfo?.id),
						)
					}
					render={(_, record) => (
						<SourceEl
							id={record.sourceInfo?.id ?? 0}
							name={record.sourceInfo?.name ?? '?'}
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
						String(values[a.id ?? 0]).localeCompare(
							String(values[b.id ?? 0]),
						)
					}
					render={(_, record: TagInfo) => (
						<TagValueEl value={values[record.id ?? 0]} />
					)}
				/>
			)}
		</Table>
	) : (
		<></>
	)
}
