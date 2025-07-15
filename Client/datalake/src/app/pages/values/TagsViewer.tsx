import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import { TagValuesViewer } from '@/app/components/values/TagValuesViewer'
import { FlattenedNestedTagsType } from '@/app/pages/values/types/flattenedNestedTags'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'

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
			<TagValuesViewer relations={relations} tagMapping={tagMapping} />
		</>
	)
})

export default TagsViewer
