import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagInfo, TagResolution, TagType } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const TagsCalculatedList = () => {
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			store.api
				.tagsGetAll({ sourceId: SourceType.Calculated })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [store.api])

	const createTag = useCallback(() => {
		store.api
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
	}, [store.api, getTags])

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader right={[<Button onClick={createTag}>Создать вычисляемый тег</Button>]}>
				Список вычисляемых тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} showState={true} />
		</>
	)
}

export default TagsCalculatedList
