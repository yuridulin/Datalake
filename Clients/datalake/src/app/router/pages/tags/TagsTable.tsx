import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import TagReceiveStateEl from '@/app/components/TagReceiveStateEl'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { compareDateStrings, compareRecords } from '@/functions/compareValues'
import { SourceSimpleInfo, TagQuality, TagSimpleInfo, TagStatusInfo } from '@/generated/data-contracts'
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
	const [search, setSearch] = useState('')

	// Получаем значения и статусы из store (реактивно через MobX)
	const tagIds = useMemo(() => viewingTags.map((x) => x.id), [viewingTags])
	const valuesRequest = useMemo(
		() => [{ requestKey: CLIENT_REQUESTKEY, tagsId: tagIds }],
		[tagIds],
	)
	const valuesResponse = useMemo(
		() => (tagIds.length > 0 ? store.valuesStore.getValues(valuesRequest) : []),
		[tagIds.length, valuesRequest, store.valuesStore],
	)
	const values = useMemo(() => {
		if (valuesResponse.length === 0) return {}
		return Object.fromEntries(valuesResponse[0].tags.map((x) => [x.id, x.values[0]]))
	}, [valuesResponse])

	// Получаем статусы только если они отображаются
	const statuses = useMemo(
		() => (showState ? store.valuesStore.getStatus(tagIds) : {}),
		[showState, tagIds, store.valuesStore],
	)
	const states = useMemo(() => {
		if (!showState) return {}
		const statesMap: Record<number, TagStatusInfo> = {}
		tagIds.forEach((tagId) => {
			const status = statuses[tagId]
			if (status) {
				statesMap[tagId] = {
					tagId,
					status,
					isError: status !== 'Ok',
				}
			}
		})
		return statesMap
	}, [statuses, tagIds, showState])

	const loadValues = useCallback(async () => {
		if (tagIds.length === 0) return
		const promises = [store.valuesStore.refreshValues(valuesRequest)]
		// Запрашиваем статусы только если они отображаются
		if (showState) {
			promises.push(store.valuesStore.refreshStatus(tagIds))
		}
		await Promise.all(promises)
	}, [store.valuesStore, valuesRequest, tagIds, showState])

	// Получаем источники из store (реактивно через MobX)
	const sourcesData = store.sourcesStore.getSources()
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

	const doSearch = useCallback(() => {
		setViewingTags(
			search.length > 0 ? tags.filter((x) => !!x.name && x.name.toLowerCase().includes(search.toLowerCase())) : tags,
		)
	}, [search, tags])

	useEffect(doSearch, [doSearch, search, tags])
	return (
		<>
			{viewingTags.length ? <PollingLoader pollingFunction={loadValues} interval={5000} /> : <></>}
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
							sorter={(a: TagSimpleInfo, b: TagSimpleInfo) => compareRecords(values[a.id], values[b.id])}
							render={(_, record: TagSimpleInfo) => {
								const value = values[record.id]
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
								compareDateStrings(values[a.id]?.date, values[b.id]?.date)
							}
							render={(_, record: TagSimpleInfo) => {
								const value = values[record.id]
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
							const stateA = states[a.id]
							const stateB = states[b.id]
							const statusA = stateA?.isError ? stateA.status ?? '' : ''
							const statusB = stateB?.isError ? stateB.status ?? '' : ''
							return statusA.localeCompare(statusB)
						}}
						render={(_, record: TagSimpleInfo) => {
							const state = states[record.id]
							const errorMessage = state?.isError ? state.status ?? undefined : undefined
							return <TagReceiveStateEl receiveState={errorMessage} />
						}}
					/>
				)}
			</Table>
		</>
	)
})

export default TagsTable
