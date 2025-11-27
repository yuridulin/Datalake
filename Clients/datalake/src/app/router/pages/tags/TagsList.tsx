import PageHeader from '@/app/components/PageHeader'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { observer } from 'mobx-react-lite'
import { useEffect, useRef } from 'react'
import TagsTable from './TagsTable'

const Tags = observer(() => {
	useDatalakeTitle('Теги')
	const store = useAppStore()
	const tags = store.tagsStore.getTags()
	const hasLoadedRef = useRef(false)

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true
		store.tagsStore.refreshTags()
		store.sourcesStore.refreshSources()
	}, [store.tagsStore, store.sourcesStore])

	return (
		<>
			<PageHeader>Список тегов</PageHeader>
			<TagsTable tags={tags} />
		</>
	)
})

export default Tags
