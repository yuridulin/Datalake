import QueryTreeSelect from '@/app/components/tagTreeSelect/QueryTreeSelect'
import { FlattenedNestedTagsType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagsValuesViewer, { ViewerSettings } from '@/app/components/values/TagsValuesViewer'
import { printDate } from '@/functions/dateHandle'
import getTagResolutionName, { TagResolutionMode } from '@/functions/getTagResolutionName'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { Row } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useMemo, useState } from 'react'

const baseTitle = 'Просмотр'

const TagsViewer = observer(() => {
	const [tagMapping, setTagMapping] = useState({} as FlattenedNestedTagsType)
	const [relations, setRelations] = useState<number[]>([])
	const [settings, setSettings] = useState<ViewerSettings | null>()

	const handleTagChange = useCallback((value: number[], currentTagMapping: FlattenedNestedTagsType) => {
		setTagMapping(currentTagMapping)
		setRelations(value)
	}, [])

	const handleSettingsChange = useCallback((settings: ViewerSettings) => {
		if (settings) setSettings(settings)
	}, [])

	const title = useMemo(() => {
		if (!settings) return baseTitle
		if (settings.mode === 'live') return `${baseTitle}: теги [${relations.length}] текущие`
		else if (settings.mode === 'exact')
			return `${baseTitle}: теги [${relations.length}] на ${printDate(settings.exact)}`
		else if (settings.mode === 'old-young')
			return `${baseTitle}: теги [${relations.length}] c ${printDate(settings.old)} по ${printDate(settings.old)} ${getTagResolutionName(settings.resolution, TagResolutionMode.Integrated)}`
		else return baseTitle
	}, [relations, settings])

	useDatalakeTitle(title)

	return (
		<>
			<Row>
				<QueryTreeSelect onChange={handleTagChange} />
			</Row>
			<TagsValuesViewer relations={relations} tagMapping={tagMapping} onChange={handleSettingsChange} />
		</>
	)
})

export default TagsViewer
