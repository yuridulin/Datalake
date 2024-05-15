import { Button } from 'antd'
import { useEffect, useState } from 'react'
import api from '../../../api/api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import { CustomSources } from '../../../etc/customSources'
import Header from '../../small/Header'
import TagsTable from './TagsTable'

export default function TagsManualList() {
	const [tags, setTags] = useState([] as TagInfo[])

	function load() {
		api.tagsReadAll({ sources: [CustomSources.Manual] }).then((res) =>
			setTags(res.data),
		)
	}

	function createManual() {
		api.tagsCreate({
			tagType: TagType.String,
			sourceId: CustomSources.Manual,
		}).then(() => load())
	}

	useEffect(load, [])

	return (
		<>
			<Header
				right={<Button onClick={createManual}>Добавить тег</Button>}
			>
				Список тегов ручного ввода
			</Header>
			{tags.length > 0 ? (
				<TagsTable tags={tags} />
			) : (
				<i>Не создано ни одного тега</i>
			)}
		</>
	)
}
