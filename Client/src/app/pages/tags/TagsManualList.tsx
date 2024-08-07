import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { CustomSource } from '../../../api/models/customSource'
import api from '../../../api/swagger-api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'
import TagsTable from './TagsTable'

export default function TagsManualList() {
	const [tags, setTags] = useState([] as TagInfo[])

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api.tagsReadAll({ sourceId: CustomSource.Manual })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	function createTag() {
		api.tagsCreate({
			sourceId: CustomSource.Manual,
			tagType: TagType.Number,
		})
			.then(() => getTags())
			.catch()
	}

	useEffect(getTags, [getTags])

	return (
		<>
			<Header
				right={
					<Button onClick={createTag}>Создать мануальный тег</Button>
				}
			>
				Список мануальных тегов
			</Header>
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}
