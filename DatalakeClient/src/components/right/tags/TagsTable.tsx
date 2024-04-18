import { useEffect, useState } from "react"
import { Tag } from "../../../@types/Tag"
import { Button, Input, Table } from 'antd'
import TagType from "../../small/TagTypeEl"
import Column from "antd/es/table/Column"
import TagValueEl from "../../small/TagValueEl"
import SourceEl from "../../small/SourceEl"
import { HistoryResponse } from "../../../@types/HistoryResponse"
import { useInterval } from "../../../hooks/useInterval"
import axios from "axios"
import { API } from "../../../router/api"
import { NavLink } from "react-router-dom"

interface TagsTableProps {
	tags: Tag[]
	hideSource?: boolean
	hideValue?: boolean
	hideType?: boolean
}

export default function TagsTable({ tags, hideSource = false, hideValue = false, hideType = false }: TagsTableProps) {

	const [ data, setData ] = useState([] as Tag[])
	const [ search, setSearch ] = useState('')
	const [ values, setValues ] = useState({} as { [key: number]: any})

	function prepareValues() {
		setValues(tags.reduce((a, tag) => ({ ...a, [tag.Id]: '' }), {}))
	}

	function loadValues() {
		axios.post(API.tags.getLiveValues, { request: { tags: tags.map(x => x.Id) }})
			.then(res => res.status === 200 && setValues(
				(res.data as HistoryResponse[])
					.reduce((a, v) => ({ ...a, [v.Id]: v.Values[0].Value }), {})))
	}

	function doSearch() {
		setData(tags.filter(x => x.Name.toLowerCase().includes(search.toLowerCase())))
	}

	
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(doSearch, [search])
	useEffect(prepareValues, [tags])
	useInterval(loadValues, 1000)

	return tags.length > 0 
		? <Table size="middle" dataSource={data} showSorterTooltip={false} >
				<Column
					title={
						<Input
							placeholder="Поиск по имени тега"
							value={search}
							onClick={e => {
								e.preventDefault()
								e.stopPropagation()
							}}
							onChange={e => {
								setSearch(e.target.value)
							}
						} />
					}
					dataIndex="Name"
					key="Name"
					defaultSortOrder="ascend"
					sorter={(a: Tag, b: Tag) => a.Name.localeCompare(b.Name)}
					render={(_, tag) => <NavLink to={`/tags/${tag.Id}`}><Button>{tag.Name}</Button></NavLink>}
					/>
				{!hideType && <Column
					title="Тип"
					dataIndex="Type"
					key="Type"
					defaultSortOrder="ascend"
					sorter={(a: Tag, b: Tag) => Number(a.Type) - Number(b.Name)}
					render={(_, record) => <TagType tagType={record.Type} />}
					/>}
				{!hideSource && <Column
					title="Источник"
					dataIndex="SourceId"
					key="SourceId"
					defaultSortOrder="ascend"
					sorter={(a: Tag, b: Tag) => (a.Source?.Name ?? String(a.SourceId)).localeCompare((b.Source?.Name ?? String(b.SourceId)))} 
					render={(_, record) => <SourceEl id={record.Source?.Id ?? 0} name={record.Source?.Name ?? '?'} />}
					/>}
				<Column
					title="Описание"
					dataIndex="Description"
					key="Description"
					defaultSortOrder="ascend"
					sorter={(a: Tag, b: Tag) => a.Description.localeCompare(b.Description)}
					/>
				{!hideValue && <Column
					title="Значение"
					dataIndex="Value"
					key="Value"
					defaultSortOrder="ascend"
					sorter={(a: Tag, b: Tag) => String(values[a.Id]).localeCompare(String(values[b.Id]))}
					render={(_, record: Tag) => <TagValueEl value={values[record.Id]} />}
					/>}
			</Table>
		: <></>
}