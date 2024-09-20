import { PlusCircleOutlined } from '@ant-design/icons'
import { Button, Table, TableColumnsType, Tag } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	SourceEntryInfo,
	SourceType,
	TagQuality,
	TagType,
} from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'
import TagCompactValue from '../../components/TagCompactValue'

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

	const columns: TableColumnsType<SourceEntryInfo> = [
		{
			dataIndex: ['itemInfo', 'item'],
			title: 'Путь в источнике',
			render: (_, record) => (
				<>{record.itemInfo?.path ?? <Tag>Путь не существует</Tag>}</>
			),
		},
		{
			dataIndex: ['itemInfo', 'value'],
			title: 'Последнее значение',
			render: (_, record) =>
				record.itemInfo ? (
					<TagCompactValue
						type={record.itemInfo.type}
						quality={TagQuality.Good}
						value={record.itemInfo.value}
					/>
				) : (
					<></>
				),
		},
		{
			dataIndex: ['tagInfo', 'guid'],
			title: 'Сопоставленный тег',
			render: (_, record) =>
				record.tagInfo ? (
					<span>
						<NavLink to={'/tags/' + record.tagInfo.guid}>
							<Button>{record.tagInfo.name}</Button>
						</NavLink>
					</span>
				) : (
					<span>
						<Button
							icon={<PlusCircleOutlined />}
							onClick={() =>
								createTag(
									record.itemInfo?.path ?? '',
									record.itemInfo?.type || TagType.String,
								)
							}
						></Button>
					</span>
				),
		},
	]

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

	if (type !== newType)
		return <>Тип источника изменен. Сохраните, чтобы продолжить</>

	return err || items.length === 0 ? (
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
						<Button onClick={read}>Обновить</Button>
					</>
				}
			>
				Доступные значения с этого источника данных
			</Header>
			<Table
				dataSource={items}
				columns={columns}
				size='small'
				pagination={false}
				rowKey='itemInfo'
			/>
		</>
	)
}
