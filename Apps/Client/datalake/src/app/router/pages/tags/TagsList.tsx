import PageHeader from '@/app/components/PageHeader'
import { TagInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const Tags = () => {
	const store = useAppStore()
	const [tags, setTags] = useState([] as TagInfo[])

	const getTags = useCallback(() => {
		setTags((prevTags) => {
			store.api
				.tagsGetAll()
				.then((res) => setTags(res.data))
				.catch(() => setTags([]))
			return prevTags
		})
	}, [store.api])

	useEffect(getTags, [getTags])

	return (
		<>
			<PageHeader>Список тегов</PageHeader>
			<TagsTable tags={tags} />
		</>
	)
}

export default Tags
