import { PlusCircleOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/api'
import {
	SourceEntryInfo,
	SourceType,
	TagType,
} from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
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

	return error ? (
		<div>
			<i>
				Источник данных не предоставил информацию о доступных значениях
			</i>
		</div>
	) : (
		<>
			<div className='table'>
				<div className='table-caption'>
					Доступные значения с этого источника данных
				</div>
				{items.map((x, i) => (
					<div className='table-row' key={i}>
						<span>{x.itemInfo?.path}</span>
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
