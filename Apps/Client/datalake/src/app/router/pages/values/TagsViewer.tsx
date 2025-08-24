import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import TagsValuesViewer from '@/app/components/values/TagsValuesViewer'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'
import { FlattenedNestedTagsType } from './types/flattenedNestedTags'

const TagsViewer = observer(() => {
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)
	const [relations, setRelations] = useState<number[]>([])

	const handleTagChange = useCallback((value: number[], currentTagMapping: FlattenedNestedTagsType) => {
		setTagMapping(currentTagMapping)
		setRelations(value)
	}, [])

	return (
		<>
			<Row>
				<QueryTreeSelect onChange={handleTagChange} />
			</Row>
			<TagsValuesViewer relations={relations} tagMapping={tagMapping} />
		</>
	)
})

export default TagsViewer
