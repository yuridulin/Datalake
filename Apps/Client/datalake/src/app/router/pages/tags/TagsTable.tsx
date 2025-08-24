import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import compareValues from '@/functions/compareValues'
import { TagInfo, TagQuality, ValueRecord } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useCallback, useEffect, useState } from 'react'
import { useInterval } from 'react-use'

interface TagsTableProps {
	tags: TagInfo[]
	hideSource?: boolean
	hideValue?: boolean
}

const TagsTable = ({ tags, hideSource = false, hideValue = false }: TagsTableProps) => {
	const store = useAppStore()
	const [viewingTags, setViewingTags] = useState(tags)
	const [search, setSearch] = useState('')
	const [values, setValues] = useState({} as { [key: number]: ValueRecord | null })

	const loadValues = () => {
		store.api
			.valuesGet([{ requestKey: CLIENT_REQUESTKEY, tagsId: viewingTags.map((x) => x.id) }])
			.then((res) => {
				setValues(Object.fromEntries(res.data[0].tags.map((x) => [x.id, x.values[0]])))
			})
			.catch(() => setValues(Object.fromEntries(Object.keys(viewingTags).map((prop) => [prop, null]))))
	}

	const doSearch = useCallback(() => {
		setViewingTags(
			search.length > 0 ? tags.filter((x) => !!x.name && x.name.toLowerCase().includes(search.toLowerCase())) : tags,
		)
	}, [search, tags])

	useEffect(doSearch, [doSearch, search, tags])
	useEffect(loadValues, [viewingTags])
	useInterval(loadValues, 5000)

	return (
		<Table size='small' dataSource={viewingTags} showSorterTooltip={false} rowKey='id'>
			<Column<TagInfo>
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
				sorter={(a: TagInfo, b: TagInfo) => (a.name ?? '').localeCompare(b.name ?? '')}
				render={(_, tag) => <TagButton tag={tag} />}
			/>
			<Column<TagInfo>
				title='Описание'
				dataIndex='description'
				sorter={(a: TagInfo, b: TagInfo) => (a.description ?? '').localeCompare(b.description ?? '')}
			/>
			{!hideSource && (
				<Column<TagInfo>
					title='Источник'
					dataIndex='sourceId'
					width='18em'
					sorter={(a: TagInfo, b: TagInfo) =>
						(a.sourceName ?? String(a.sourceId)).localeCompare(b.sourceName ?? String(b.sourceId))
					}
					render={(_, record) => <SourceButton source={{ id: record.sourceId ?? 0, name: record.sourceName ?? '?' }} />}
				/>
			)}
			{!hideValue && (
				<>
					<Column<TagInfo>
						title='Значение'
						width='12em'
						sorter={(a: TagInfo, b: TagInfo) => compareValues(values[a.id]?.value, values[b.id]?.value)}
						render={(_, record: TagInfo) => {
							const value = values[record.id]
							return (
								<TagCompactValue
									value={value?.value}
									type={record.type}
									quality={value?.quality ?? TagQuality.BadNoConnect}
								/>
							)
						}}
					/>
					<Column<TagInfo>
						title='Дата записи'
						width='13em'
						sorter={(a: TagInfo, b: TagInfo) => compareValues(values[a.id]?.date, values[b.id]?.date)}
						render={(_, record: TagInfo) => {
							const value = values[record.id]
							return value?.dateString
						}}
					/>
				</>
			)}
		</Table>
	)
}

export default TagsTable
