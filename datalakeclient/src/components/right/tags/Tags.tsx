import { useEffect, useState } from "react"
import tagsApi from "../../../api/tagsApi"
import { useFetching } from "../../../hooks/useFetching"
import { Tag } from "../../../@types/tag"
import { Button } from 'antd'
import TagCreate from "./TagCreate"
import TagUpdate from "./TagUpdate"

export default function Tags() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ id, setId ] = useState('')
	const [ isCreate, setIsCreate ] = useState(false)
	const [ isUpdate, setIsUpdate ] = useState(false)

	const [ load, , error ] = useFetching(async () => {
		let res = await tagsApi.list()
		if (res) setTags(res)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])

	return (
		error
			? <div>Произошла ошибка</div>
			: <>
				<div>
					<Button onClick={() => setIsCreate(true)}>Добавить тег</Button>
				</div>
				<table className="view-table">
					<thead>
						<tr>
							<th>Имя</th>
							<th>Описание</th>
							<th>Источник</th>
						</tr>
					</thead>
					<tbody>
					{tags.map(x =>
						<tr onClick={() => { setId(x.TagName); setIsUpdate(true) }}>
							<td>{x.TagName}</td>
							<td>{x.Description}</td>
							<td>{x.SourceId}</td>
						</tr>
					)}
					</tbody>
				</table>
				<TagCreate visible={isCreate} setVisible={setIsCreate} loadTable={load} />
				<TagUpdate tagName={id} visible={isUpdate} setVisible={setIsUpdate} loadTable={load} />
			</>
	)
}