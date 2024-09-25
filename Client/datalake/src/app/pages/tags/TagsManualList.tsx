import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { CustomSource } from '../../../api/models/customSource'
import api from '../../../api/swagger-api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import CreatedTagLinker from '../../components/CreatedTagsLinker'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

export default function TagsManualList() {
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

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
			.then((res) => {
				getTags()
				setCreated(res.data)
			})
			.catch()
	}

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader
				right={
					<Button onClick={createTag}>Создать мануальный тег</Button>
				}
			>
				Список мануальных тегов
			</PageHeader>
			{!!created && (
				<CreatedTagLinker
					tag={created}
					onClose={() => setCreated(null)}
				/>
			)}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}
