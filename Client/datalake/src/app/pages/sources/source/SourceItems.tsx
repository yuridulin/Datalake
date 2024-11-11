import api from '@/api/swagger-api'
import { PlusCircleOutlined } from '@ant-design/icons'
import { Button, Input, Table, TableColumnsType, Tag } from 'antd'
import debounce from 'debounce'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import {
	SourceEntryInfo,
	SourceType,
	TagInfo,
	TagType,
} from '../../../../api/swagger/data-contracts'
import compareValues from '../../../../functions/compareValues'
import CreatedTagLinker from '../../../components/CreatedTagsLinker'
import PageHeader from '../../../components/PageHeader'
import TagCompactValue from '../../../components/TagCompactValue'
import routes from '../../../router/routes'

type SourceItemsProps = {
	type: SourceType
	newType: SourceType
	id: number
}

const SourceItems = ({ type, newType, id }: SourceItemsProps) => {
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)
	const [created, setCreated] = useState(null as TagInfo | null)
	const [search, setSearch] = useState('')

	const columns: TableColumnsType<SourceEntryInfo> = [
		{
			dataIndex: ['itemInfo', 'item'],
			title: 'Путь в источнике',
			render: (_, record) => (
				<>{record.itemInfo?.path ?? <Tag>Путь не существует</Tag>}</>
			),
			sorter: (a, b) => compareValues(a.itemInfo?.path, b.itemInfo?.path),
		},
		{
			dataIndex: ['itemInfo', 'value'],
			title: 'Последнее значение',
			render: (_, record) =>
				record.itemInfo ? (
					<TagCompactValue
						type={record.itemInfo.type}
						quality={record.itemInfo.quality}
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
						<NavLink
							to={routes.tags.toTagForm(record.tagInfo.guid)}
						>
							<Button>{record.tagInfo.name}</Button>
						</NavLink>
					</span>
				) : (
					<span>
						<Button
							size='small'
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
			if (!res.data?.id) return
			read()
			setCreated(res.data)
		})
	}

	const doSearch = debounce((value: string) => {
		const tokens = value
			.toLowerCase()
			.split(' ')
			.filter((x) => x.length > 0)
		console.log('ищем по строкам:', tokens)
		if (value.length > 0) {
			setSearchedItems(
				items.filter(
					(x) =>
						tokens.filter(
							(token) =>
								token.length > 0 &&
								(
									(x.itemInfo?.path ?? '') +
									(x.tagInfo?.name ?? '')
								)
									.toLowerCase()
									.indexOf(token) > -1,
						).length == tokens.length,
				),
			)
		} else {
			setSearchedItems(items)
		}
	}, 300)

	useEffect(
		function () {
			doSearch(search)
		},
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[items],
	)
	useEffect(() => {
		console.log(
			'Нашлось элементов: ' +
				searchedItems.length +
				' из ' +
				items.length,
		)
	}, [searchedItems, items])
	useEffect(read, [id])

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
			<PageHeader
				right={
					<>
						<Button onClick={read}>Обновить</Button>
					</>
				}
			>
				Доступные значения с этого источника данных
			</PageHeader>
			{!!created && (
				<CreatedTagLinker
					tag={created}
					onClose={() => setCreated(null)}
				/>
			)}
			<Input.Search
				style={{ marginBottom: '1em' }}
				placeholder='Введите запрос для поиска по значениям и тегам. Можно написать несколько запросов, разделив пробелами'
				value={search}
				onChange={(e) => {
					setSearch(e.target.value)
					doSearch(e.target.value.toLowerCase())
				}}
			/>
			<Table
				dataSource={searchedItems}
				columns={columns}
				showSorterTooltip={false}
				size='small'
				pagination={{ position: ['bottomCenter'] }}
				rowKey={(row) =>
					(row.itemInfo?.path ?? '') + (row.tagInfo?.guid ?? '')
				}
			/>
		</>
	)
}

export default SourceItems
