import { Button, Input } from "antd";
import Header from "../../small/Header";
import FormRow from "../../small/FormRow";
import { NavLink } from "react-router-dom";
import TagType from "../../small/TagType";
import axios from "axios";
import { API } from "../../../router/api";
import { useEffect, useState } from "react";
import { Tag } from "../../../@types/Tag";
import { ManualId } from "../../../@types/enums/CustomSourcesIdentity";
import { useUpdateContext } from "../../../context/updateContext";

export default function Manual() {

	const { setUpdate } = useUpdateContext()
	const [ tags, setTags ] = useState([] as Tag[])
	const [ search, setSearch ] = useState('')

	function load() {
		axios.get(API.tags.getManualTags)
			.then(res => setTags(res.data))
	}

	function createManual() {
		axios.post(API.tags.create, { sourceId: ManualId })
			.then(res => {
				if (res.data.Done) {
					load()
					setUpdate(new Date())
				}
			})
	}

	useEffect(load, [])

	return <>
		<Header
			right={<Button onClick={createManual}>Добавить тег</Button>}
		>Список тегов ручного ввода</Header>
		<FormRow title="Поиск">
			<Input value={search} onChange={e => setSearch(e.target.value)} placeholder="введите поисковый запрос..." />
		</FormRow>
		{tags.length === 0
			? <i>нет ни одного тега</i>
			: 
			<div className="table">
				<div className="table-header">
					<span>Имя</span>
					<span>Тип</span>
					<span>Описание</span>
				</div>
				{tags.filter(x => (x.Description + x.Name + x.SourceItem).toLowerCase().trim().includes(search.toLowerCase())).map(x =>
					<NavLink className="table-row" to={'/tags/' + x.Id} key={x.Id}>
						<span>{x.Name}</span>
						<span>
							<TagType tagType={x.Type} />
						</span>
						<span>{x.Description}</span>
					</NavLink>
				)}
			</div>
		}
	</>
}