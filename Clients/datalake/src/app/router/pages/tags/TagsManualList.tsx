import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagType, TagWithSettingsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'
import TagsTable from './TagsTable'

const TagsManualList = observer(() => {
	useDatalakeTitle('Теги', 'Мануальные')
	const store = useAppStore()
	// Получаем теги из store (реактивно через MobX)
	const tags = store.tagsStore.getTags(SourceType.Manual)
	const [created, setCreated] = useState(null as TagWithSettingsInfo | null)

	const createTag = useCallback(async () => {
		try {
			const response = await store.api.inventoryTagsCreate({
				sourceId: SourceType.Manual,
				tagType: TagType.Number,
			})
			// Инвалидируем кэш и обновляем данные
			if (response.data) {
				store.tagsStore.invalidateTag(response.data.id)
				await store.tagsStore.refreshTags(SourceType.Manual)
				setCreated(response.data)
			}
		} catch (error) {
			console.error('Failed to create tag:', error)
		}
	}, [store])

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
