import { PlusCircleOutlined } from '@ant-design/icons'
import { Button, Tag } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	SourceEntryInfo,
	SourceType,
	TagType,
} from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import Header from '../../small/Header'
import TagTypeEl from '../../small/TagTypeEl'

export default function SourceItems({
	type,
	id,
}: {
	type: SourceType
	id: number
}) {
	const [items, setItems] = useState([] as SourceEntryInfo[])

	const [read, , error] = useFetching(async () => {
		api.sourcesGetItemsWithTags(id).then((res) => {
			setItems(res.data)
		})
	})

	useEffect(() => {
		!!id && read()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	const createTag = async (item: string, tagType: TagType) => {
		api.tagsCreate({
			name: '',
			tagType: tagType,
			sourceId: id,
			sourceItem: item,
		}).then((res) => {
			if (res.data > 0) read()
		})
	}

	const [createEmptyTag] = useFetching(async () => {
		api.tagsCreate({
			name: '',
			tagType: TagType.String,
			sourceId: Number(id),
		}).then((res) => {
			if (res.data > 0) read()
		})
	})

	if (type !== SourceType.Datalake && type !== SourceType.Inopc) return <></>

	return error ? (
		<div>
			<i>
				Источник данных не предоставил информацию о доступных значениях
			</i>
		</div>
	) : (
		<>
			<Header
				right={
					<>
						<Button onClick={createEmptyTag}>Добавить тег</Button>
						<Button onClick={read}>Обновить</Button>
					</>
				}
			>
				Доступные значения с этого источника данных
			</Header>
			<div className='table'>
				{items.map((x, i) => (
					<div className='table-row' key={i}>
						<span>
							{x.itemInfo?.path ?? <Tag>Путь не существует</Tag>}
						</span>
						<span>
							<TagTypeEl
								tagType={x.itemInfo?.type || TagType.String}
							/>
						</span>
						{!!x.tagInfo ? (
							<span>
								<NavLink to={'/tags/' + x.tagInfo.id}>
									<Button>{x.tagInfo.name}</Button>
								</NavLink>
							</span>
						) : (
							<span>
								<Button
									icon={<PlusCircleOutlined />}
									onClick={() =>
										createTag(
											x.itemInfo?.path ?? '',
											x.itemInfo?.type || TagType.String,
										)
									}
								></Button>
							</span>
						)}
					</div>
				))}
			</div>
		</>
	)
}
