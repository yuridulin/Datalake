import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import TagsValuesWriter from '@/app/components/values/TagsValuesWriter'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'
import { FlattenedNestedTagsType } from './types/flattenedNestedTags'

const TagsWriter = observer(() => {
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)
	const [relations, setRelations] = useState<number[]>([])

	const handleTagChange = useCallback((value: number[], currentTagMapping: FlattenedNestedTagsType) => {
		setTagMapping(currentTagMapping)
		setRelations(value)
	}, [])

	return (
		<>
			<Row style={{ marginBottom: '.5em' }}>
				<QueryTreeSelect onChange={handleTagChange} />
			</Row>
			<TagsValuesWriter relations={relations} tagMapping={tagMapping} />
		</>
	)
})

export default TagsWriter
