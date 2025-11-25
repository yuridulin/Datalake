import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import PageHeader from '@/app/components/PageHeader'
import { SourceType, TagType, TagWithSettingsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import TagsTable from './TagsTable'

const TagsCalculatedList = observer(() => {
	useDatalakeTitle('Теги', 'Вычисляемые')
	const store = useAppStore()
	// Получаем теги из store (реактивно через MobX)
	const tags = store.tagsStore.getTags(SourceType.Calculated)
	const [created, setCreated] = useState(null as TagWithSettingsInfo | null)

	const createTag = useCallback(async () => {
		try {
			const response = await store.api.inventoryTagsCreate({
				sourceId: SourceType.Calculated,
				tagType: TagType.Number,
			})
			// Инвалидируем кэш и обновляем данные
			if (response.data) {
				store.tagsStore.invalidateTag(response.data.id)
				await store.tagsStore.refreshTags(SourceType.Calculated)
				setCreated(response.data)
			}
		} catch (error) {
			console.error('Failed to create tag:', error)
		}
	}, [store])

	// Обновляем данные при переходе на страницу
	useEffect(() => {
		store.tagsStore.refreshTags(SourceType.Calculated).catch(console.error)
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
