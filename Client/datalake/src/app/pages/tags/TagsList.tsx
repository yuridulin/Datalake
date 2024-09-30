import { useCallback, useEffect, useState } from 'react'
import api from '../../../api/swagger-api'
import { TagInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

export default function Tags() {
	const [tags, setTags] = useState([] as TagInfo[])

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api.tagsReadAll()
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader>Список тегов</PageHeader>
			<TagsTable tags={tags} />
		</>
	)
}
