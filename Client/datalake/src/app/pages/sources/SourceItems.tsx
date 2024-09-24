import { PlusCircleOutlined } from '@ant-design/icons'
import { Button, Table, TableColumnsType, Tag } from 'antd'
import { createStyles } from 'antd-style'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	SourceEntryInfo,
	SourceType,
	TagQuality,
	TagType,
} from '../../../api/swagger/data-contracts'
import compareValues from '../../../hooks/compareValues'
import { useInterval } from '../../../hooks/useInterval'
import Header from '../../components/Header'
import TagCompactValue from '../../components/TagCompactValue'

const useStyle = createStyles(({ css, prefixCls }) => {
	return {
		customTable: css`
			${prefixCls}-table {
				${prefixCls}-table-container {
					${prefixCls}-table-body,
					${prefixCls}-table-content {
						scrollbar-width: thin;
						scrollbar-color: unset;
					}
				}
			}
		`,
	}
})

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
	const { styles } = useStyle()

	const columns: TableColumnsType<SourceEntryInfo> = [
		{
			dataIndex: ['itemInfo', 'item'],
			title: 'Путь в источнике',
			render: (_, record) => (
				<>{record.itemInfo?.path ?? <Tag>Путь не существует</Tag>}</>
			),
			sorter: (a, b) => compareValues(a.itemInfo?.path, b.itemInfo?.path),
			defaultSortOrder: 'ascend',
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
			sorter: (a, b) =>
				compareValues(a.itemInfo?.value, b.itemInfo?.value),
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
			sorter: (a, b) => compareValues(a.tagInfo?.name, b.tagInfo?.name),
		},
	]

	function read() {
		if (!id) return
		api.sourcesGetItemsWithTags(id)
			.then((res) => {
				setItems(res.data)
				setErr(false)
			})
			.catch(() => setErr(true))
	}

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

	useEffect(read, [id])
	useInterval(read, 5000)

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
				className={styles.customTable}
				size='small'
				pagination={false}
				rowKey='itemInfo'
				scroll={{ y: 55 * 8 }}
			/>
		</>
	)
}
