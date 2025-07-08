import api from '@/api/swagger-api'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { SourceType, TagFrequency, TagInfo, TagType } from '../../../api/swagger/data-contracts'
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

	function createTag() {
		api
			.tagsCreate({
				sourceId: SourceType.Manual,
				tagType: TagType.Number,
				frequency: TagFrequency.NotSet,
			})
			.then((res) => {
				getTags()
				setCreated(res.data)
			})
			.catch()
	}

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader right={<Button onClick={createTag}>Создать мануальный тег</Button>}>
				Список мануальных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}

export default TagsManualList
