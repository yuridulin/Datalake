import api from '@/api/swagger-api'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { SourceType, TagInfo, TagResolution, TagType } from '../../../generated/data-contracts'
import CreatedTagLinker from '../../components/CreatedTagsLinker'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

const TagsCalculatedList = () => {
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api
				.tagsGetAll({ sourceId: SourceType.Calculated })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	function createTag() {
		api
			.tagsCreate({
				sourceId: SourceType.Calculated,
				tagType: TagType.Number,
				resolution: TagResolution.NotSet,
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
			<PageHeader right={<Button onClick={createTag}>Создать вычисляемый тег</Button>}>
				Список вычисляемых тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}

export default TagsCalculatedList
