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
import Header from '../../components/Header'
import TagTypeEl from '../../components/TagTypeEl'
import TagValueEl from '../../components/TagValueEl'

export default function SourceItems({
	type,
	newType,
	id,
}: {
	type: SourceType
	newType: SourceType
	id: number
}) {
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)

	function read() {
		api.sourcesGetItemsWithTags(id)
			.then((res) => {
				setItems(res.data)
				setErr(false)
			})
			.catch(() => setErr(true))
	}

	useEffect(() => {
		if (!id) return
		read()
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

	function createEmptyTag() {
		api.tagsCreate({
			name: '',
			tagType: TagType.String,
			sourceId: Number(id),
		}).then((res) => {
			if (res.data > 0) read()
		})
	}

	if (type !== newType)
		return <>Тип источника изменен. Сохраните, чтобы продолжить</>

	return err ? (
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
				<div className='table-header'>
					<span>Путь в источнике</span>
					<span>Тип значения</span>
					<span title='Значение, полученное при опросе источника'>
						Значение
					</span>
					<span>Тег</span>
				</div>
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
						<span>
							<TagValueEl
								type={x.itemInfo?.type || TagType.String}
								allowEdit={false}
								value={x.itemInfo?.value}
							/>
						</span>
						{x.tagInfo ? (
							<span>
								<NavLink to={'/tags/' + x.tagInfo.guid}>
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
