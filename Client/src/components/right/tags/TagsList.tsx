import { useEffect, useState } from 'react'
import api from '../../../api/swagger-api'
import { TagInfo } from '../../../api/swagger/data-contracts'
import Header from '../../small/Header'
import TagsTable from './TagsTable'

export default function Tags() {
	const [tags, setTags] = useState([] as TagInfo[])

	function load() {
		api.tagsReadAll().then((res) => setTags(res.data))
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(load, [])

	return (
		<>
			<Header>Список тегов</Header>
			{tags.length > 0 ? (
				<TagsTable tags={tags} />
			) : (
				<i>Не создано ни одного тега</i>
			)}
		</>
	)
}
