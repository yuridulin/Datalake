import { useEffect, useState } from "react"
import { Tag } from "../../../@types/Tag"
import { Button } from 'antd'
import axios from "axios"
import Header from "../../small/Header"
import { API } from "../../../router/api"
import { useNavigate } from "react-router-dom"
import TagsTable from "./TagsTable"

export default function Tags() {

	const navigate = useNavigate()
	const [ tags, setTags ] = useState([] as Tag[])
	
	function load() {
		axios.post(API.tags.getFlatList).then(res => res.status === 200 && setTags(res.data))
	}

	function createTag() { 
		axios.post(API.tags.create).then(res => res.data.Done && navigate(`/tags/${res.data.Data}`))
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(load, [])

	return (
		<>
			<Header
				right={<Button onClick={createTag}>Добавить тег</Button>}
			>Список тегов</Header>
			{tags.length > 0 
				? <TagsTable tags={tags} />
				: <i>Не создано ни одного тега</i>
			}
		</>
	)
}