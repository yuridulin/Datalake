import PageHeader from '@/app/components/PageHeader'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { observer } from 'mobx-react-lite'
import TagsTable from './TagsTable'

const Tags = observer(() => {
	useDatalakeTitle('Теги')
	const store = useAppStore()
	// Получаем теги из store (реактивно через MobX)
	const tags = store.tagsStore.getTags()

	return (
		<>
			<PageHeader>Список тегов</PageHeader>
			<TagsTable tags={tags} />
		</>
	)
})

export default Tags
