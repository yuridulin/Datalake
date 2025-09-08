import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import TagsValuesWriter from '@/app/components/values/TagsValuesWriter'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'
import { FlattenedNestedTagsType } from './types/flattenedNestedTags'

const TagsWriter = observer(() => {
	// хочу отобразить настройки из QueryTreeSelect (кол-во выбранных тегов) и TagsValuesWriter (время) и менять по необходимости
	useDatalakeTitle('Запись', 'Теги {count} текущие/на {exact}')

	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)
	const [relations, setRelations] = useState<number[]>([])

	const handleTagChange = useCallback((value: number[], currentTagMapping: FlattenedNestedTagsType) => {
		setTagMapping(currentTagMapping)
		setRelations(value)
	}, [])

	return (
		<>
			<Row style={{ marginBottom: '.5em' }}>
				<QueryTreeSelect onChange={handleTagChange} manualOnly />
			</Row>
			<TagsValuesWriter relations={relations} tagMapping={tagMapping} />
		</>
	)
})

export default TagsWriter
