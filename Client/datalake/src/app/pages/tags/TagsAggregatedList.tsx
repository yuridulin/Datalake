import api from '@/api/swagger-api'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { SourceType, TagFrequency, TagInfo, TagType } from '../../../api/swagger/data-contracts'
import CreatedTagLinker from '../../components/CreatedTagsLinker'
import PageHeader from '../../components/PageHeader'
import TagsTable from './TagsTable'

const TagsAggregatedList = () => {
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			api
				.tagsReadAll({ sourceId: SourceType.Aggregated })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [])

	const createTag = () => {
		api
			.tagsCreate({
				sourceId: SourceType.Aggregated,
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
			<PageHeader right={<Button onClick={createTag}>Создать вычисляемый тег</Button>}>
				Список агрегированных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}

export default TagsAggregatedList
