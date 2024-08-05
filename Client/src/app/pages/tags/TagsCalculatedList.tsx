import { Button, Input } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../../api/models/customSource'
import api from '../../../api/swagger-api'
import { TagInfo, TagType } from '../../../api/swagger/data-contracts'
import FormRow from '../../components/FormRow'
import Header from '../../components/Header'
import TagTypeEl from '../../components/TagTypeEl'

export default function TagsCalculatedList() {
	const [tags, setTags] = useState([] as TagInfo[])
	const [search, setSearch] = useState('')

	function load() {
		api.tagsReadAll({ sourceId: CustomSource.Calculated }).then((res) =>
			setTags(res.data),
		)
	}

	function create() {
		api.tagsCreate({
			sourceId: CustomSource.Calculated,
			tagType: TagType.Number,
		}).then(() => load())
	}

	useEffect(load, [])

	return (
		<>
			<Header right={<Button onClick={create}>Добавить тег</Button>}>
				Список вычисляемых тегов
			</Header>
			<FormRow title='Поиск'>
				<Input
					value={search}
					onChange={(e) => setSearch(e.target.value)}
					placeholder='введите поисковый запрос...'
				/>
			</FormRow>
			{tags.length === 0 ? (
				<i>нет ни одного тега</i>
			) : (
				<div className='table'>
					<div className='table-header'>
						<span>Имя</span>
						<span>Тип</span>
						<span>Описание</span>
					</div>
					{tags
						.filter((x) =>
							(
								(x.description ?? '') +
								x.name +
								(x.sourceName ?? '')
							)
								.toLowerCase()
								.trim()
								.includes(search.toLowerCase()),
						)
						.map((x) => (
							<NavLink
								className='table-row'
								to={'/tags/' + x.guid}
								key={x.guid}
							>
								<span>{x.name}</span>
								<span>
									<TagTypeEl tagType={x.type} />
								</span>
								<span>{x.description}</span>
							</NavLink>
						))}
				</div>
			)}
		</>
	)
}
