import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { CustomSource } from '../../../api/models/customSource'
import api from '../../../api/swagger-api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'
import TagsTable from './TagsTable'

export default function TagsManualList() {
	const [tags, setTags] = useState([] as TagInfo[])

	function load() {
		api.tagsReadAll({ sourceId: CustomSource.Manual }).then((res) =>
			setTags(res.data),
		)
	}

	function createManual() {
		api.tagsCreate({
			tagType: TagType.String,
			sourceId: CustomSource.Manual,
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
