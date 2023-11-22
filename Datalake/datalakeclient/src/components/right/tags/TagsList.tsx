import { useEffect, useState } from "react"
import { Tag } from "../../../@types/Tag"
import { Button, Input } from 'antd'
import TagType from "../../small/TagTypeEl"
import axios from "axios"
import Header from "../../small/Header"
import { NavLink } from "react-router-dom"
import FormRow from "../../small/FormRow"
import { TagSource } from "../../../@types/Source"
import { API } from "../../../router/api"
import { CalculatedId, ManualId } from "../../../@types/enums/CustomSourcesIdentity"

export default function Tags() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ sources, setSources ] = useState([] as TagSource[])
	const [ search, setSearch ] = useState('')

	function load() {
		axios.post(API.tags.getFlatList).then(res => setTags(res.data))
		axios.post(API.sources.list).then(res => setSources(res.data))
	}

	function createTag() { 
		axios.post(API.tags.create).then(res => res.data.Done && load())
	}

	const SourceEl = ({ id }: { id: number}) => {
		if (id === ManualId) {
			return <NavLink to={`/tags/manual/`}>
				<Button>Мануальный</Button>
			</NavLink>
		}
		else if (id === CalculatedId) {
			return <NavLink to={`/tags/calc/`}>
				<Button>Вычисляемый</Button>
			</NavLink>
		}
		else {
			let finded = sources.filter(x => x.Id === id)
			if (finded.length > 0) {
				return <NavLink to={`/sources/${finded[0].Id}`}>
					<Button>{finded[0].Name}</Button>
				</NavLink>
			}
			else {
				return <span>?</span>
			}
		}
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])
	//useEffect(() => { }, [search])

	return (
		<>
			<Header
				right={<Button onClick={createTag}>Добавить тег</Button>}
			>Список тегов</Header>
			{tags.length > 0 
				? <>
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
							<div className="table-row" key={x.Id}>
								<span>
									<NavLink to={'/tags/' + x.Id}>
										<Button>{x.Name}</Button>
									</NavLink>
								</span>
								<span><TagType tagType={x.Type} /></span>
								<span><SourceEl id={x.SourceId} /></span>
								<span>{x.Description}</span>
							</div>
						)}
					</div>
				</>
				: <i>Не создано ни одного тега</i>
			}
		</>
	)
}