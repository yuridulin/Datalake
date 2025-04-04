import api from '@/api/swagger-api'
import { useCallback, useEffect, useState } from 'react'
import { SourceType, TagInfo } from '../../../api/swagger/data-contracts'
import CreatedTagLinker from '../../components/CreatedTagsLinker'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

const TagsManualList = () => {
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api
				.tagsReadAll({ sourceId: SourceType.Manual })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader>Список мануальных тегов</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}

export default TagsManualList
