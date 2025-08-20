import api from '@/api/swagger-api'
import { useCallback, useEffect, useState } from 'react'
import { TagInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

const Tags = () => {
	const [tags, setTags] = useState([] as TagInfo[])

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api
				.tagsGetAll()
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

export default Tags
