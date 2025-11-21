import PageHeader from '@/app/components/PageHeader'
import { TagInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { useEffect, useRef, useState } from 'react'
import TagsTable from './TagsTable'

const Tags = () => {
	useDatalakeTitle('Теги')
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])
	const hasLoadedRef = useRef(false)

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true

		store.api
			.inventoryTagsGetAll()
			.then((res) => setTags(res.data))
			.catch(() => setTags([]))
	}, [store.api])

	return (
		<>
			<PageHeader>Список тегов</PageHeader>
			<TagsTable tags={tags} />
		</>
	)
}

export default Tags
