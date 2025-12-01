import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagSimpleInfo, TagType } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useRef, useState } from 'react'
import TagsTable from './TagsTable'

const TagsCalculatedList = observer(() => {
	useDatalakeTitle('Теги', 'Вычисляемые')
	const store = useAppStore()
	const tags = store.tagsStore.calculatedTags
	const [created, setCreated] = useState(null as TagSimpleInfo | null)
	const hasLoadedRef = useRef(false)

	const createTag = useCallback(async () => {
		try {
			const createdTag = await store.tagsStore.createTag({
				sourceId: SourceType.Calculated,
				tagType: TagType.Number,
			})
			if (createdTag) {
				setCreated(createdTag)
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create tag'), {
				component: 'TagsCalculatedList',
				action: 'createTag',
			})
		}
	}, [store])

	useEffect(() => {
		if (hasLoadedRef.current) return
		hasLoadedRef.current = true
		store.tagsStore.refreshTags()
	}, [store.tagsStore])

	return (
		<>
			<PageHeader right={[<Button onClick={createTag}>Создать вычисляемый тег</Button>]}>
				Список вычисляемых тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} showState={true} />
		</>
	)
})

export default TagsCalculatedList
