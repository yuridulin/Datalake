import TagButton from '@/app/components/buttons/TagButton'
import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import compareValues from '@/functions/compareValues'
import {
	SourceEntryInfo,
	SourceInfo,
	SourceTagInfo,
	SourceUpdateRequest,
	TagInfo,
	TagResolution,
	TagType,
} from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import {
	CheckCircleOutlined,
	CloseCircleOutlined,
	DownOutlined,
	MinusCircleOutlined,
	PlusCircleOutlined,
} from '@ant-design/icons'
import { Alert, Button, Col, Input, Popconfirm, Radio, Row, Table, TableColumnsType, Tag, theme, Tree } from 'antd'
import debounce from 'debounce'
import { useEffect, useState } from 'react'

interface GroupedEntry {
	path: string
	itemInfo?: SourceEntryInfo['itemInfo']
	tagInfoArray: SourceTagInfo[]
}

function groupEntries(items: SourceEntryInfo[]): GroupedEntry[] {
	const map = items.reduce<Record<string, GroupedEntry>>((acc, x) => {
		const key = x.itemInfo?.path || '__no_path__'
		if (!acc[key]) {
			acc[key] = { path: key, itemInfo: x.itemInfo, tagInfoArray: [] }
		}
		if (x.tagInfo) {
			acc[key].tagInfoArray.push(x.tagInfo)
		}
		return acc
	}, {})

	return Object.values(map)
}

interface TreeNodeData {
	key: string
	title: React.ReactNode
	children?: TreeNodeData[]
	isLeaf: boolean
	path: string
	group?: GroupedEntry // Добавлено поле group
	countLeaves: number
	countTags: number
}

type SourceItemsProps = {
	source: SourceInfo
	request: SourceUpdateRequest
}

// Функция для форматирования чисел
const formatCount = (count: number) => (count > 0 ? `: ${count}` : ' нет')

const localStorageKey = 'sourceItemsViewMode'

const SourceItems = ({ source, request }: SourceItemsProps) => {
	const store = useAppStore()
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)
	const [created, setCreated] = useState(null as TagInfo | null)
	const [search, setSearch] = useState('')
	const [viewMode, setViewMode] = useState<'table' | 'tree'>(() => {
		const saved = localStorage.getItem(localStorageKey) as 'table' | 'tree' | null
		return saved || 'table'
	})
	const { token } = theme.useToken()

	// Функция для построения дерева
	const buildTree = (groups: GroupedEntry[]): TreeNodeData[] => {
		// Интерфейс для временного узла при построении дерева
		interface TempTreeNode {
			children: Record<string, TempTreeNode>
			countLeaves: number
			countTags: number
			path: string
			group?: GroupedEntry
			isLeaf?: boolean
		}

		// создаём единый корень
		const root: TempTreeNode = {
			children: {},
			countLeaves: 0,
			countTags: 0,
			path: '',
		}

		groups.forEach((group) => {
			const parts = group.path === '__no_path__' ? ['Без пути'] : group.path.split('.')
			let current = root

			parts.forEach((part, idx) => {
				if (!current.children[part]) {
					current.children[part] = {
						children: {},
						countLeaves: 0,
						countTags: 0,
						path: parts.slice(0, idx + 1).join('.'),
					}
				}
				current = current.children[part]
			})

			// leaf: сохраняем инфо
			current.group = group
			current.isLeaf = true
			current.countLeaves = 1
			current.countTags = group.tagInfoArray.length
		})

		// рекурсивная сборка дерева
		const buildNode = (node: TempTreeNode, keyPath: string): TreeNodeData[] => {
			return Object.entries(node.children).map(([name, child]) => {
				const fullKey = `${keyPath}.${name}`
				const isLeaf = !!child.isLeaf

				// считаем суммарные метрики
				const sub = buildNode(child, fullKey)
				const leavesCount = sub.reduce((sum, n) => sum + n.countLeaves, 0) + (child.isLeaf ? 1 : 0)
				const tagsCount = sub.reduce((sum, n) => sum + n.countTags, 0) + (child.countTags || 0)

				// заголовок: если лист, на первой строке — value
				const title = isLeaf ? (
					<div style={{ display: 'flex', flexDirection: 'row' }}>
						{/* 2) Имя узла */}
						<span>{name}</span>
						&emsp;
						{/* 1) Значение */}
						{child.group?.itemInfo && (
							<TagCompactValue
								type={child.group.itemInfo.type}
								quality={child.group.itemInfo.quality}
								value={child.group.itemInfo.value}
							/>
						)}
					</div>
				) : (
					<div style={{ display: 'flex', alignItems: 'center' }}>
						<span style={{ flex: 1 }}>{name}</span>&emsp;
						<span style={{ color: token.colorTextSecondary, fontSize: '0.85em' }}>
							значений{formatCount(leavesCount)}, тегов{formatCount(tagsCount)}
						</span>
					</div>
				)

				return {
					key: fullKey,
					title,
					path: child.path,
					isLeaf,
					countLeaves: leavesCount,
					countTags: tagsCount,
					children: sub.length > 0 ? sub : undefined,
					group: child.group, // Сохраняем группу в узле
				}
			})
		}

		return buildNode(root, 'root')
	}

	// Рендер содержимого для листового узла
	const renderLeafContent = (group?: GroupedEntry) => {
		if (!group) return null

		return (
			<div style={{ marginLeft: '1.5em' }}>
				{/* все связанные теги */}
				{group.tagInfoArray.map((tag) => (
					<div key={tag.id} style={{ marginTop: '.5em', display: 'flex', alignItems: 'center' }}>
						<TagButton tag={tag} />
						<Popconfirm
							title={
								<>
									Вы уверены, что хотите удалить тег?
									<br />
									Убедитесь, что он не используется где-то еще
								</>
							}
							onConfirm={() => deleteTag(tag.id)}
							okText='Да'
							cancelText='Нет'
						>
							<Button size='small' icon={<MinusCircleOutlined />} style={{ marginLeft: 8 }} />
						</Popconfirm>
					</div>
				))}
				{/* кнопка создания нового тега */}
				{group.itemInfo && (
					<div style={{ marginTop: '.5em', display: 'flex', alignItems: 'center' }}>
						<Button
							size='small'
							icon={<PlusCircleOutlined />}
							onClick={() => createTag(group.path, group.itemInfo!.type || TagType.String)}
						>
							Создать тег
						</Button>
					</div>
				)}
			</div>
		)
	}

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
		store.api
			.sourcesGetItemsWithTags(source.id)
			.then((res) => {
				setItems(res.data)
				setErr(false)
			})
			.catch(() => setErr(true))
	}

	const createTag = async (item: string, tagType: TagType) => {
		store.api
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
		store.api.tagsDelete(tagId).then(read)
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

	useEffect(() => {
		localStorage.setItem(localStorageKey, viewMode)
	}, [viewMode])

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
						<Col flex='14em'>
							&emsp;
							{/* Переключатель режимов просмотра */}
							<Radio.Group value={viewMode} onChange={(e) => setViewMode(e.target.value)} style={{ marginRight: 16 }}>
								<Radio.Button value='table'>Таблица</Radio.Button>
								<Radio.Button value='tree'>Дерево</Radio.Button>
							</Radio.Group>
						</Col>
						<Col flex='6em'>
							<Button onClick={read}>Обновить</Button>
						</Col>
					</Row>

					{viewMode === 'table' ? (
						<Table
							dataSource={searchedItems}
							columns={columns}
							showSorterTooltip={false}
							size='small'
							pagination={{ position: ['bottomCenter'] }}
							rowKey={(row) => (row.itemInfo?.path ?? '') + (row.tagInfo?.guid ?? '')}
						/>
					) : (
						<Tree
							showLine
							switcherIcon={<DownOutlined />}
							treeData={buildTree(groupEntries(searchedItems))}
							titleRender={(node) => (
								<div>
									{node.title}
									{node.isLeaf && renderLeafContent(node.group)} {/* Передаем group */}
								</div>
							)}
						/>
					)}
				</>
			)}
		</>
	)
}

export default SourceItems
