import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import { TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagsValuesWriter from '@/app/components/values/TagsValuesWriter'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'

const TagsWriter = observer(() => {
	// хочу отобразить настройки из QueryTreeSelect (кол-во выбранных тегов) и TagsValuesWriter (время) и менять по необходимости
	useDatalakeTitle('Запись', 'Теги {count} текущие/на {exact}')

	const [tagMapping, setTagMapping] = useState({} as TagMappingType)
	const [relations, setRelations] = useState<string[]>([])

	const handleTagChange = useCallback((value: string[], currentTagMapping: TagMappingType) => {
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
