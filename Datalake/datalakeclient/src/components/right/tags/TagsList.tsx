import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { Tag } from "../../../@types/Tag"
import { Button, Input } from 'antd'
import TagType from "../../small/TagType"
import axios from "axios"
import Header from "../../small/Header"
import { NavLink } from "react-router-dom"
import FormRow from "../../small/FormRow"

export default function Tags() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ search, setSearch ] = useState('')

	const [ load,, error ] = useFetching(async () => {
		let res = await axios.post('tags/list')
		setTags(res.data)
	})

	const [ createTag ] = useFetching(async () => {
		let res = await axios.post('tags/create')
		if (res.data.Done) load()
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])
	//useEffect(() => { }, [search])

	return (
		error
			? <div>Произошла ошибка</div>
			: <>
				<Header
					right={<Button onClick={createTag}>Добавить тег</Button>}
				>Список тегов</Header>
				<FormRow title="Поиск">
					<Input value={search} onChange={e => setSearch(e.target.value)} placeholder="введите поисковый запрос..." />
				</FormRow>
				<div className="table">
					<div className="table-header">
						<span>Имя</span>
						<span>Тип</span>
						<span>Источник</span>
						<span>Описание</span>
					</div>
					{tags.filter(x => (x.Description + x.Name + x.SourceItem).toLowerCase().trim().includes(search.toLowerCase())).map(x =>
						<NavLink className="table-row" to={'/tags/' + x.Id} key={x.Id}>
							<span>{x.Name}</span>
							<span>
								<TagType tagType={x.Type} />
							</span>
							<span>{x.Source}</span>
							<span>{x.Description}</span>
						</NavLink>
					)}
				</div>
			</>
	)
}