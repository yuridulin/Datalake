import { useCallback, useEffect, useState } from 'react'
import api from '../../../api/swagger-api'
import { TagInfo } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'
import TagsTable from './TagsTable'

export default function Tags() {
	const [tags, setTags] = useState([] as TagInfo[])

	const loadAllTagsList = useCallback(() => {
		setTags((prevTags) => {
			api.tagsReadAll()
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	useEffect(loadAllTagsList, [loadAllTagsList])

	return (
		<>
			<Header>Список тегов</Header>
			<TagsTable tags={tags} />
		</>
	)
}
