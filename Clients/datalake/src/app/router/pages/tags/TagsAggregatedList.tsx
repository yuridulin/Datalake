import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagInfo, TagType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const TagsAggregatedList = () => {
	useDatalakeTitle('Теги', 'Агрегированные')
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			store.api
				.inventoryTagsGetAll({ sourceId: SourceType.Aggregated })
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [store.api])

	const createTag = useCallback(() => {
		store.api
			.inventoryTagsCreate({
				sourceId: SourceType.Aggregated,
				tagType: TagType.Number,
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
				Список агрегированных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} showState={true} />
		</>
	)
}

export default TagsAggregatedList
