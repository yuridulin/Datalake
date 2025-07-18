import api from '@/api/swagger-api'
import TagButton from '@/app/components/buttons/TagButton'
import { CheckCircleOutlined, CloseCircleOutlined, MinusCircleOutlined, PlusCircleOutlined } from '@ant-design/icons'
import { Alert, Button, Col, Input, Popconfirm, Row, Table, TableColumnsType, Tag, theme } from 'antd'
import debounce from 'debounce'
import { useEffect, useState } from 'react'
import {
	SourceEntryInfo,
	SourceInfo,
	SourceUpdateRequest,
	TagInfo,
	TagResolution,
	TagType,
} from '../../../../api/swagger/data-contracts'
import compareValues from '../../../../functions/compareValues'
import CreatedTagLinker from '../../../components/CreatedTagsLinker'
import TagCompactValue from '../../../components/TagCompactValue'

type SourceItemsProps = {
	source: SourceInfo
	request: SourceUpdateRequest
}

const SourceItems = ({ source, request }: SourceItemsProps) => {
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)
	const [created, setCreated] = useState(null as TagInfo | null)
	const [search, setSearch] = useState('')
	const { token } = theme.useToken()

	const columns: TableColumnsType<SourceEntryInfo> = [
		{
			dataIndex: ['itemInfo', 'item'],
			title: 'Путь в источнике',
			width: '30%',
			render: (_, record) => <>{record.itemInfo?.path ?? <Tag>Путь не существует</Tag>}</>,
			sorter: (a, b) => compareValues(a.itemInfo?.path, b.itemInfo?.path),
		},
		{
			dataIndex: ['itemInfo', 'value'],
			title: 'Последнее значение',
			width: '20em',
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
			sorter: (a, b) => compareValues(a.itemInfo?.value, b.itemInfo?.value),
		},
		{
			dataIndex: ['tagInfo', 'guid'],
			title: 'Сопоставленные теги',
			render: (_, record) =>
				!record.tagInfo ? (
					<span>
						<Button
							size='small'
							icon={<PlusCircleOutlined />}
							onClick={() => createTag(record.itemInfo?.path ?? '', record.itemInfo?.type || TagType.String)}
						></Button>
					</span>
				) : (
					<span>
						<TagButton tag={record.tagInfo} />
						<Popconfirm
							title={
								<>
									Вы уверены, что хотите удалить тег?
									<br />
									Убедитесь, что он не используется где-то еще, перед удалением
								</>
							}
							placement='bottom'
							onConfirm={() => deleteTag(record.tagInfo!.id)}
							okText='Да'
							cancelText='Нет'
						>
							<Button size='small' icon={<MinusCircleOutlined />}></Button>
						</Popconfirm>
					</span>
				),
			sorter: (a, b) => compareValues(a.tagInfo?.name, b.tagInfo?.name),
		},
		{
			dataIndex: ['isTagInUse'],
			width: '3em',
			align: 'center',
			title: (
				<span title='Показывает, используется ли тег в запросах получения данных, внутренних или внешних. Подробнее о запросах можно узнать, перейдя на страницу просмотра информации о теге'>
					Исп.
				</span>
			),
			render: (_, record) =>
				record.tagInfo ? (
					record.isTagInUse ? (
						<CheckCircleOutlined style={{ color: token.colorSuccess }} title='Тег используется' />
					) : (
						<CloseCircleOutlined title='Тег не используется' />
					)
				) : (
					<></>
				),
			sorter: (a, b) => compareValues(a.tagInfo && a.isTagInUse, b.tagInfo && b.isTagInUse),
		},
	]

	function read() {
		if (!source.id) return
		api
			.sourcesGetItemsWithTags(source.id)
			.then((res) => {
				setItems(res.data)
				setErr(false)
			})
			.catch(() => setErr(true))
	}

	const createTag = async (item: string, tagType: TagType) => {
		api
			.tagsCreate({
				name: '',
				tagType: tagType,
				sourceId: source.id,
				sourceItem: item,
				resolution: TagResolution.Minute,
			})
			.then((res) => {
				if (!res.data?.id) return
				setCreated(res.data)
				setItems(
					items.map((x) =>
						x.itemInfo?.path === item
							? {
									...x,
									tagInfo: {
										id: res.data.id,
										guid: res.data.guid,
										item: res.data.sourceItem ?? item,
										name: res.data.name,
										sourceType: source.id,
										type: res.data.type,
										accessRule: { ruleId: 0, access: 0 },
										formulaInputs: [],
										resolution: res.data.resolution,
									},
								}
							: x,
					),
				)
			})
	}

	const deleteTag = (tagId: number) => {
		api.tagsDelete(tagId).then(read)
	}

	const doSearch = debounce((value: string) => {
		const tokens = value
			.toLowerCase()
			.split(' ')
			.filter((x) => x.length > 0)
		if (value.length > 0) {
			setSearchedItems(
				items.filter(
					(x) =>
						tokens.filter(
							(token) =>
								token.length > 0 &&
								((x.itemInfo?.path ?? '') + (x.tagInfo?.name ?? '')).toLowerCase().indexOf(token) > -1,
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
	useEffect(read, [source])

	if (source.address !== request.address || source.type !== request.type)
		return <Alert message='Тип источника изменен. Сохраните, чтобы продолжить' />

	return err ? (
		<Alert message='Ошибка при получении данных' />
	) : (
		<>
			{items.length === 0 ? (
				<div>
					<i>Источник данных не предоставил информацию о доступных значениях</i>
				</div>
			) : (
				<>
					{!!created && <CreatedTagLinker tag={created} onClose={() => setCreated(null)} />}
					<Row>
						<Col flex='auto'>
							<Input.Search
								style={{ marginBottom: '1em', alignItems: 'center', justifyContent: 'space-between' }}
								placeholder='Введите запрос для поиска по значениям и тегам. Можно написать несколько запросов, разделив пробелами'
								value={search}
								onChange={(e) => {
									setSearch(e.target.value)
									doSearch(e.target.value.toLowerCase())
								}}
							/>
						</Col>
						<Col>
							<Button onClick={read}>Обновить</Button>
						</Col>
					</Row>
					<Table
						dataSource={searchedItems}
						columns={columns}
						showSorterTooltip={false}
						size='small'
						pagination={{ position: ['bottomCenter'] }}
						rowKey={(row) => (row.itemInfo?.path ?? '') + (row.tagInfo?.guid ?? '')}
					/>
				</>
			)}
		</>
	)
}

export default SourceItems
