import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagType, TagWithSettingsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useRef, useState } from 'react'
import TagsTable from './TagsTable'

const TagsManualList = observer(() => {
	useDatalakeTitle('Теги', 'Мануальные')
	const store = useAppStore()
	const tags = store.tagsStore.getTags(SourceType.Manual)
	const [created, setCreated] = useState(null as TagWithSettingsInfo | null)
	const hasLoadedRef = useRef(false)

	const createTag = useCallback(async () => {
		try {
			const response = await store.api.inventoryTagsCreate({
				sourceId: SourceType.Manual,
				tagType: TagType.Number,
			})
			if (response.data) {
				store.tagsStore.invalidateTag(response.data.id)
				store.tagsStore.refreshTags(SourceType.Manual)
				setCreated(response.data)
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create tag'), {
				component: 'TagsManualList',
				action: 'createTag',
			})
		}
	}, [store])

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true
		store.tagsStore.refreshTags(SourceType.Manual)
	}, [store.tagsStore])

	return (
		<>
			<PageHeader right={[<Button onClick={createTag}>Создать мануальный тег</Button>]}>
				Список мануальных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} />
		</>
	)
})

export default TagsManualList
