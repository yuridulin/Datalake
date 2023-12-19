import { useEffect, useState } from "react"
import { Tag } from "../../../@types/Tag"
import { Button, Input, Table } from 'antd'
import TagType from "../../small/TagTypeEl"
import axios from "axios"
import Header from "../../small/Header"
import { TagSource } from "../../../@types/Source"
import { API } from "../../../router/api"
import Column from "antd/es/table/Column"
import TagValueEl from "../../small/TagValueEl"
import SourceEl from "../../small/SourceEl"

export default function Tags() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ sources, setSources ] = useState([] as TagSource[])
	const [ data, setData ] = useState([] as Tag[])
	const [ search, setSearch ] = useState('')

	function load() {
		axios.post(API.tags.getFlatList).then(res => {
			setTags(res.data)
			setData(res.data)
		})
		axios.post(API.sources.list).then(res => setSources(res.data))
	}

	function createTag() { 
		axios.post(API.tags.create).then(res => res.data.Done && load())
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(load, [])
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(function () { setData(tags.filter(x => x.Name.includes(search))) }, [search])

	return (
		<>
			<Header
				right={<Button onClick={createTag}>Добавить тег</Button>}
			>Список тегов</Header>
			{tags.length > 0 
				? <>
					<Table size="middle" dataSource={data} showSorterTooltip={false}>
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
							/>
						<Column
							title="Тип"
							dataIndex="Type"
							key="Type"
							defaultSortOrder="ascend"
							sorter={(a: Tag, b: Tag) => Number(a.Type) - Number(b.Name)}
							render={(_, record) => <TagType tagType={record.Type} />} />
						<Column
							title="Источник"
							dataIndex="SourceId"
							key="SourceId"
							defaultSortOrder="ascend"
							sorter={(a: Tag, b: Tag) => (a.Source?.Name ?? String(a.SourceId)).localeCompare((b.Source?.Name ?? String(b.SourceId)))} 
							render={(_, record) => <SourceEl sources={sources} id={record.SourceId} />}
							/>
						<Column
							title="Описание"
							dataIndex="Description"
							key="Description"
							defaultSortOrder="ascend"
							sorter={(a: Tag, b: Tag) => a.Description.localeCompare(b.Description)} />
						<Column
							title="Значение"
							dataIndex="Value"
							key="Value"
							render={(_, record: Tag) => <TagValueEl value={record.Value} />} />
					</Table>
				</>
				: <i>Не создано ни одного тега</i>
			}
		</>
	)
}