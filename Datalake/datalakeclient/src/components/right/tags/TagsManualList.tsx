import { Button } from "antd"
import Header from "../../small/Header"
import { useNavigate } from "react-router-dom"
import axios from "axios"
import { API } from "../../../router/api"
import { useEffect, useState } from "react"
import { Tag } from "../../../@types/Tag"
import { ManualId } from "../../../@types/enums/CustomSourcesIdentity"
import TagsTable from "./TagsTable"

export default function TagsManualList() {

	const navigate = useNavigate()
	const [ tags, setTags ] = useState([] as Tag[])

	function load() {
		axios.get(API.tags.getManualTags)
			.then(res => setTags(res.data))
	}

	function createManual() {
		axios.post(API.tags.create, { sourceId: ManualId })
			.then(res => res.status === 200 && navigate(`/tags/${res.data.Data}`))
	}

	useEffect(load, [])

	return <>
		<Header
			right={<Button onClick={createManual}>Добавить тег</Button>}
		>Список тегов ручного ввода</Header>
		{tags.length > 0 
				? <TagsTable tags={tags} />
				: <i>Не создано ни одного тега</i>
			}
	</>
}