import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagInfo, TagResolution, TagType } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const TagsManualList = () => {
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			store.api
				.tagsGetAll({ sourceId: SourceType.Manual })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [store.api])

	const createTag = useCallback(() => {
		store.api
			.tagsCreate({
				sourceId: SourceType.Manual,
				tagType: TagType.Number,
				resolution: TagResolution.NotSet,
			})
			.then((res) => {
				getTags()
				setCreated(res.data)
			})
			.catch()
	}, [store.api, getTags])

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader right={[<Button onClick={createTag}>Создать мануальный тег</Button>]}>
				Список мануальных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
}

export default TagsManualList
