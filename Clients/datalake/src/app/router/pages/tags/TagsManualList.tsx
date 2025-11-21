import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagInfo, TagType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { useCallback, useEffect, useRef, useState } from 'react'
import TagsTable from './TagsTable'

const TagsManualList = () => {
	useDatalakeTitle('Теги', 'Мануальные')
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])
	const [created, setCreated] = useState(null as TagInfo | null)
	const hasLoadedRef = useRef(false)

	const getTags = useCallback(() => {
		store.api
			.inventoryTagsGetAll({ sourceId: SourceType.Manual })
			.then((res) => setTags(res.data))
			.catch(() => setTags([]))
	}, [store.api])

	const createTag = useCallback(() => {
		store.api
			.inventoryTagsCreate({
				sourceId: SourceType.Manual,
				tagType: TagType.Number,
			})
			.then((res) => {
				getTags()
				setCreated(res.data)
			})
			.catch()
	}, [store.api, getTags])

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true
		getTags()
	}, [getTags])

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
