import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import TagReceiveStateEl from '@/app/components/TagReceiveStateEl'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { compareDateStrings, compareRecords } from '@/functions/compareValues'
import { SourceSimpleInfo, TagQuality, TagSimpleInfo, TagStatusInfo, ValueRecord } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useMemo, useState } from 'react'

interface TagsTableProps {
	tags: TagSimpleInfo[]
	hideSource?: boolean
	hideValue?: boolean
	showState?: boolean
}

const TagsTable = observer(({ tags, hideSource = false, hideValue = false, showState = false }: TagsTableProps) => {
	const store = useAppStore()
	const [viewingTags, setViewingTags] = useState(tags)
	const tagsId = useMemo(() => viewingTags.map((x) => x.id), [viewingTags])

	// Значения
	const [values, setValues] = useState(new Map<number, ValueRecord>())

	// Состояния расчета
	const [collectStates, setCollectStates] = useState(new Map<number, TagStatusInfo>())

	// Функция обновления
	const pollingFunc = useCallback(() => {
		store.api
			.dataValuesGet([{ requestKey: CLIENT_REQUESTKEY, tagsId: tagsId }])
			.then((res) => {
				const map = new Map<number, ValueRecord>()
				for (const tagWithValue of res.data[0].tags) {
					map.set(tagWithValue.id, tagWithValue.values[0])
				}
				setValues(map)
			})
			.catch(() => {
				store.notify?.warning({ message: 'Ошибка при получении значений' })
			})

		store.api.dataTagsGetStatus({ tagsId: tagsId }).then((res) => {
			const map = new Map<number, TagStatusInfo>()
			for (const state of res.data) {
				map.set(state.tagId, state)
			}
			setCollectStates(map)
		})
	}, [store.api, tagsId, store.notify])

	// Данные о источниках
	const sourcesData = store.sourcesStore.sources
	const sources = useMemo(() => {
		if (hideSource) return {}
		const map: Record<number, SourceSimpleInfo> = {}
		sourcesData.forEach((source) => {
			map[source.id] = {
				id: source.id,
				name: source.name,
				type: source.type,
				accessRule: source.accessRule,
			}
		})
		// NOTE: Cпециальные системные источники уже есть в списке - как и обычные
		return map
	}, [sourcesData, hideSource])

	// Работа с поиском
	const [search, setSearch] = useState('')
	const doSearch = useCallback(() => {
		setViewingTags(
			search.length > 0 ? tags.filter((x) => !!x.name && x.name.toLowerCase().includes(search.toLowerCase())) : tags,
		)
	}, [search, tags])
	useEffect(doSearch, [doSearch, search, tags])

	// Рендер
	return (
		<>
			{viewingTags.length ? <PollingLoader pollingFunction={pollingFunc} interval={5000} /> : <></>}
			<Table size='small' dataSource={viewingTags} showSorterTooltip={false} rowKey='id'>
				<Column<TagSimpleInfo>
					title={
						<div style={{ display: 'flex', alignItems: 'center' }}>
							<div style={{ padding: '0 1em' }}>Название</div>
							<div style={{ width: '100%' }}>
								<Input
									placeholder='Поиск...'
									value={search}
									onClick={(e) => {
										e.preventDefault()
										e.stopPropagation()
									}}
									onChange={(e) => {
										setSearch(e.target.value)
									}}
								/>
							</div>
						</div>
					}
					dataIndex='name'
					defaultSortOrder='ascend'
					width='40%'
					sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => (a.name ?? '').localeCompare(b.name ?? '')}
					render={(_, tag) => <TagButton tag={tag} />}
				/>
				<Column<TagSimpleInfo>
					title='Описание'
					dataIndex='description'
					sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => (a.description ?? '').localeCompare(b.description ?? '')}
				/>
				{!hideSource && (
					<Column<TagSimpleInfo>
						title='Источник'
						dataIndex='sourceId'
						width='18em'
						sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => {
							const sourceA = sources[a.sourceId]?.name ?? String(a.sourceId)
							const sourceB = sources[b.sourceId]?.name ?? String(b.sourceId)
							return sourceA.localeCompare(sourceB)
						}}
						render={(_, record) => {
							const source = sources[record.sourceId]
							return (
								<SourceButton
									source={
										source ?? {
											id: record.sourceId ?? 0,
											name: String(record.sourceId ?? '?'),
											type: 0,
											accessRule: { ruleId: 0, access: 0 },
										}
									}
								/>
							)
						}}
					/>
				)}
				{!hideValue && (
					<>
						<Column<TagSimpleInfo>
							title='Значение'
							width='12em'
							sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => compareRecords(values.get(a.id), values.get(b.id))}
							render={(_, record: TagSimpleInfo) => {
								const value = values.get(record.id)
								if (!value) return <></>
								return (
									<TagCompactValue
										record={value}
										type={record.type}
										quality={value?.quality ?? TagQuality.BadNoConnect}
									/>
								)
							}}
						/>
						<Column<TagSimpleInfo>
							title='Дата записи'
							width='13em'
							sorter={(a: TagSimpleInfo, b: TagSimpleInfo) =>
								compareDateStrings(values.get(a.id)?.date, values.get(b.id)?.date)
							}
							render={(_, record: TagSimpleInfo) => {
								const value = values.get(record.id)
								return value?.date
							}}
						/>
					</>
				)}
				{showState && (
					<Column<TagSimpleInfo>
						title='Последний расчет'
						width='25em'
						sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => {
							const stateA = collectStates.get(a.id)
							const stateB = collectStates.get(b.id)
							const statusA = stateA?.isError ? (stateA.status ?? '') : ''
							const statusB = stateB?.isError ? (stateB.status ?? '') : ''
							return statusA.localeCompare(statusB)
						}}
						render={(_, record: TagSimpleInfo) => {
							const state = collectStates.get(record.id)
							const errorMessage = state?.isError ? (state.status ?? undefined) : undefined
							return <TagReceiveStateEl receiveState={errorMessage} />
						}}
					/>
				)}
			</Table>
		</>
	)
})

export default TagsTable
