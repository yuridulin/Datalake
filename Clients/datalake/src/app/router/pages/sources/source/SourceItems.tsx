import TagButton from '@/app/components/buttons/TagButton'
import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import { LoadStatus } from '@/app/components/loaders/loaderTypes'
import StatusLoader from '@/app/components/loaders/StatusLoader'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import compareValues from '@/functions/compareValues'
import {
	SourceTagInfo as ApiSourceTagInfo,
	SourceItemInfo,
	SourceUpdateRequest,
	SourceWithSettingsAndTagsInfo,
	TagSimpleInfo,
	TagType,
} from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import {
	CheckCircleOutlined,
	CloseCircleOutlined,
	DownOutlined,
	MinusCircleOutlined,
	PlusCircleOutlined,
} from '@ant-design/icons'
import { Alert, Button, Col, Input, Popconfirm, Radio, Row, Table, TableColumnsType, Tag, theme, Tree } from 'antd'
import dayjs from 'dayjs'
import debounce from 'debounce'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useLocalStorage } from 'react-use'

// Расширяем SourceTagInfo из API до TagSimpleInfo для совместимости с компонентами
type SourceTagInfo = TagSimpleInfo & { item?: string | null }

type SourceEntryInfo = {
	itemInfo?: SourceItemInfo
	tagInfo?: SourceTagInfo
	isTagInUse?: string
}

interface GroupedEntry {
	path: string
	itemInfo?: SourceItemInfo
	tagInfoArray: SourceTagInfo[]
}

// Преобразует SourceTagInfo из API в полный SourceTagInfo с расширенными полями TagSimpleInfo
// Использует значения из source для недостающих полей
const toSourceTagInfo = (tag: ApiSourceTagInfo, sourceId: number, sourceType: number): SourceTagInfo => {
	return {
		id: tag.id,
		guid: `tag-${tag.id}`, // Временный guid на основе id, если нужен полный guid - можно получить отдельно
		name: tag.name,
		description: null,
		type: tag.type,
		resolution: tag.resolution,
		sourceId: sourceId,
		sourceType: sourceType,
		accessRule: tag.accessRule,
		item: tag.item ?? null,
	}
}

const getLastUsage = (usage?: Record<string, string>) => {
	if (!usage) return undefined
	return Object.values(usage).reduce<string | undefined>((latest, current) => {
		if (!current) return latest
		if (!latest) return current
		return dayjs(current).isAfter(dayjs(latest)) ? current : latest
	}, undefined)
}

const mergeEntries = (
	sourceItems: SourceItemInfo[],
	tags: SourceTagInfo[],
	usage: Record<string, Record<string, string>>,
): SourceEntryInfo[] => {
	const entries: SourceEntryInfo[] = []
	const itemsByPath = new Map(sourceItems.map((item) => [item.path, item]))
	const taggedPaths = new Set<string>()

	// Сначала обрабатываем теги и собираем информацию о помеченных путях
	tags.forEach((tag) => {
		const itemPath = tag.item
		if (itemPath) taggedPaths.add(itemPath)
		const itemInfo = itemPath ? itemsByPath.get(itemPath) : undefined
		entries.push({
			itemInfo,
			tagInfo: tag,
			isTagInUse: getLastUsage(usage?.[tag.id]),
		})
	})

	// Затем добавляем items, которые не были помечены тегами
	sourceItems.forEach((item) => {
		if (!taggedPaths.has(item.path)) {
			entries.push({ itemInfo: item })
		}
	})

	return entries.sort((a, b) => {
		const pathA = a.itemInfo?.path ?? a.tagInfo?.item ?? ''
		const pathB = b.itemInfo?.path ?? b.tagInfo?.item ?? ''
		const byPath = compareValues(pathA, pathB)
		if (byPath !== 0) return byPath
		return compareValues(a.tagInfo?.name, b.tagInfo?.name)
	})
}

function groupEntries(items: SourceEntryInfo[]): GroupedEntry[] {
	const map = items.reduce<Record<string, GroupedEntry>>((acc, x) => {
		// Используем path из itemInfo, если есть, иначе из tagInfo.item
		const key = x.itemInfo?.path || x.tagInfo?.item || '__no_path__'
		if (!acc[key]) {
			acc[key] = { path: key, itemInfo: x.itemInfo, tagInfoArray: [] }
		}
		// Если itemInfo отсутствует в группе, но есть в текущем entry, добавляем его
		if (!acc[key].itemInfo && x.itemInfo) {
			acc[key].itemInfo = x.itemInfo
		}
		// Если itemInfo отсутствует, но есть tagInfo с item, и мы нашли itemInfo в другом entry,
		// обновляем path группы на правильный путь из itemInfo
		if (acc[key].itemInfo && acc[key].path !== acc[key].itemInfo.path) {
			acc[key].path = acc[key].itemInfo.path
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
	source: SourceWithSettingsAndTagsInfo
	request: SourceUpdateRequest
}

type ViewModeState = 'table' | 'tree'

// Функция для форматирования чисел
const formatCount = (count: number) => (count > 0 ? `: ${count}` : ' нет')

const localStorageKey = 'sourceItemsViewMode'

const SourceItems = ({ source, request }: SourceItemsProps) => {
	const store = useAppStore()
	const [paginationConfig, setPaginationConfig] = useLocalStorage('sourceItems-' + source.id, {
		pageSize: 10,
		current: 1,
	})
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)
	const [created, setCreated] = useState(null as SourceTagInfo | null)
	const [search, setSearch] = useState('')
	const [viewMode, setViewMode] = useLocalStorage(localStorageKey, 'table' as ViewModeState)
	const { token } = theme.useToken()
	const [status, setStatus] = useState<LoadStatus>('default')
	const hasLoadedRef = useRef(false)
	const lastSourceIdRef = useRef<number | undefined>(source.id)

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
				if (child.group?.itemInfo) {
					const record = {
						date: new Date().toDateString(),
						quality: child.group.itemInfo.quality,
						boolean: child.group.itemInfo.type === TagType.Boolean ? Boolean(child.group.itemInfo.value) : null,
						number: child.group.itemInfo.type === TagType.Number ? Number(child.group.itemInfo.value) : null,
						text: child.group.itemInfo.type === TagType.String ? String(child.group.itemInfo.value) : null,
					}
					console.log(child.group?.itemInfo, record)
				}
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
								record={{
									date: new Date().toDateString(),
									quality: child.group.itemInfo.quality,
									boolean: child.group.itemInfo.type === TagType.Boolean ? Boolean(child.group.itemInfo.value) : null,
									number: child.group.itemInfo.type === TagType.Number ? Number(child.group.itemInfo.value) : null,
									text: child.group.itemInfo.type === TagType.String ? String(child.group.itemInfo.value) : null,
								}}
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
						record={{
							date: new Date().toDateString(),
							quality: record.itemInfo.quality,
							boolean: record.itemInfo.type === TagType.Boolean ? Boolean(record.itemInfo.value) : null,
							number: record.itemInfo.type === TagType.Number ? Number(record.itemInfo.value) : null,
							text: record.itemInfo.type === TagType.String ? String(record.itemInfo.value) : null,
						}}
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
					record.isTagInUse && dayjs(record.isTagInUse).add(30, 'minute') > dayjs() ? (
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

	const reload = useCallback(() => {
		if (!source.id) return
		setStatus('loading')
		setErr(false)

		const fetchData = async () => {
			try {
				const sourceItemsResponse = await store.api.dataSourcesGetItems(source.id)

				// Используем теги из source.tags вместо запроса inventoryTagsGetAll
				const apiTags = source.tags ?? []

				// Преобразуем ApiSourceTagInfo в SourceTagInfo с полными полями TagSimpleInfo
				// Используем значения из source для недостающих полей (sourceId, sourceType)
				const tags: SourceTagInfo[] = apiTags.map((apiTag) => toSourceTagInfo(apiTag, source.id, source.type))

				const tagIds = tags.map((tag) => tag.id).filter((id): id is number => typeof id === 'number')

				let usage: Record<string, Record<string, string>> = {}
				if (tagIds.length > 0) {
					try {
						const usageResponse = await store.api.dataTagsGetUsage({ tagsId: tagIds })
						// Преобразуем массив TagUsageInfo[] в Record<string, Record<string, string>>
						usage = (usageResponse.data ?? []).reduce(
							(acc, item) => {
								if (item.tagId !== null && item.tagId !== undefined) {
									acc[String(item.tagId)] = item.requests ?? {}
								}
								return acc
							},
							{} as Record<string, Record<string, string>>,
						)
					} catch (usageError) {
						logger.error(usageError instanceof Error ? usageError : new Error('Не удалось получить usage тегов'), {
							component: 'SourceItems',
							action: 'loadSourceItems',
						})
					}
				}

				const merged = mergeEntries(sourceItemsResponse.data ?? [], tags, usage)
				setItems(merged)
				setStatus('success')
			} catch {
				setStatus('error')
				setErr(true)
			}
		}

		fetchData()
	}, [source.id, source.tags, source.type, store.api])

	const reloadDone = useCallback(() => setStatus('default'), [])

	const createTag = async (item: string, tagType: TagType) => {
		store.api
			.inventoryTagsCreate({
				name: '',
				tagType: tagType,
				sourceId: source.id,
				sourceItem: item,
			})
			.then((res) => {
				if (!res.data?.id) return
				setCreated(res.data)
				const newTag: SourceTagInfo = {
					...res.data,
					item: res.data.sourceItem ?? item,
				}
				setItems((prev) => prev.map((x) => (x.itemInfo?.path === item ? { ...x, tagInfo: newTag } : x)))
			})
	}

	const deleteTag = (tagId: number) => {
		store.api.inventoryTagsDelete(tagId).then(reload)
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
	useEffect(() => {
		// Если изменился source.id, сбрасываем флаг загрузки
		if (lastSourceIdRef.current !== source.id) {
			hasLoadedRef.current = false
			lastSourceIdRef.current = source.id
		}

		if (hasLoadedRef.current || !source.id) return
		hasLoadedRef.current = true
		reload()
	}, [store.api, source, reload])

	if (source.address !== request.address || source.type !== request.type)
		return <Alert message='Тип источника изменен. Сохраните, чтобы продолжить' />

	return (
		<>
			<StatusLoader status={status} after={reloadDone} />
			{err ? (
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
									<Radio.Group
										value={viewMode}
										onChange={(e) => setViewMode(e.target.value)}
										style={{ marginRight: 16 }}
									>
										<Radio.Button value='table'>Таблица</Radio.Button>
										<Radio.Button value='tree'>Дерево</Radio.Button>
									</Radio.Group>
								</Col>
								<Col flex='6em'>
									<Button onClick={reload}>Обновить</Button>
								</Col>
							</Row>

							{viewMode === 'table' ? (
								<Table
									dataSource={searchedItems}
									columns={columns}
									showSorterTooltip={false}
									size='small'
									pagination={{
										...paginationConfig,
										position: ['bottomCenter'],
										showSizeChanger: true,
										onChange: (page, pageSize) => {
											setPaginationConfig({ current: page, pageSize: pageSize || 10 })
										},
									}}
									rowKey={(row) => (row.itemInfo?.path ?? '') + (row.tagInfo?.guid ?? String(row.tagInfo?.id ?? ''))}
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
			)}
		</>
	)
}

export default SourceItems
