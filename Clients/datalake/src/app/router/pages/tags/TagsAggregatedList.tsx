import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagType, TagWithSettingsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const TagsAggregatedList = observer(() => {
	useDatalakeTitle('Теги', 'Агрегированные')
	const store = useAppStore()
	// Получаем теги из store (реактивно через MobX)
	const tags = store.tagsStore.getTags(SourceType.Aggregated)
	const [created, setCreated] = useState(null as TagWithSettingsInfo | null)

	const createTag = useCallback(async () => {
		try {
			const response = await store.api.inventoryTagsCreate({
				sourceId: SourceType.Aggregated,
				tagType: TagType.Number,
			})
			// Инвалидируем кэш и обновляем данные
			if (response.data) {
				store.tagsStore.invalidateTag(response.data.id)
				await store.tagsStore.refreshTags(SourceType.Aggregated)
				setCreated(response.data)
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create tag'), {
				component: 'TagsAggregatedList',
				action: 'createTag',
			})
		}
	}, [store])

	// getTags() автоматически загрузит данные при первом вызове

	return (
		<>
			<PageHeader right={[<Button onClick={createTag}>Создать вычисляемый тег</Button>]}>
				Список агрегированных тегов
			</PageHeader>
			{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
			<TagsTable tags={tags} hideSource={true} showState={true} />
		</>
	)
})

export default TagsAggregatedList
