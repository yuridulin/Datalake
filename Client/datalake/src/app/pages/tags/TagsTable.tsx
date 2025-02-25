import api from '@/api/swagger-api'
import TagButton from '@/app/components/buttons/TagButton'
import { Input, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useCallback, useEffect, useState } from 'react'
import { TagInfo, ValueRecord } from '../../../api/swagger/data-contracts'
import compareValues from '../../../functions/compareValues'
import { useInterval } from '../../../hooks/useInterval'
import SourceEl from '../../components/SourceEl'
import TagCompactValue from '../../components/TagCompactValue'

interface TagsTableProps {
	tags: TagInfo[]
	hideSource?: boolean
	hideValue?: boolean
}

const TagsTable = ({
	tags,
	hideSource = false,
	hideValue = false,
}: TagsTableProps) => {
	const [viewingTags, setViewingTags] = useState(tags)
	const [search, setSearch] = useState('')
	const [values, setValues] = useState(
		{} as { [key: number]: ValueRecord | null },
	)

	const loadValues = () => {
		api.valuesGet([
			{ requestKey: 'tags-table', tagsId: viewingTags.map((x) => x.id) },
		])
			.then((res) => {
				setValues(
					Object.fromEntries(
						res.data[0].tags.map((x) => [x.id, x.values[0]]),
					),
				)
			})
			.catch(() =>
				setValues(
					Object.fromEntries(
						Object.keys(viewingTags).map((prop) => [prop, null]),
					),
				),
			)
	}

	const doSearch = useCallback(() => {
		setViewingTags(
			search.length > 0
				? tags.filter(
						(x) =>
							!!x.name &&
							x.name.toLowerCase().includes(search.toLowerCase()),
					)
				: tags,
		)
	}, [search, tags])

	useEffect(doSearch, [doSearch, search, tags])
	useEffect(loadValues, [viewingTags])
	useInterval(loadValues, 5000)

	return (
		<Table
			size='small'
			dataSource={viewingTags}
			showSorterTooltip={false}
			rowKey='id'
		>
			<Column<TagInfo>
				title={
					<Input
						placeholder='Поиск по имени тега'
						value={search}
						onClick={(e) => {
							e.preventDefault()
							e.stopPropagation()
						}}
						onChange={(e) => {
							setSearch(e.target.value)
						}}
					/>
				}
				dataIndex='name'
				defaultSortOrder='ascend'
				width='40%'
				sorter={(a: TagInfo, b: TagInfo) =>
					(a.name ?? '').localeCompare(b.name ?? '')
				}
				render={(_, tag) => <TagButton tag={tag} />}
			/>
			<Column<TagInfo>
				title='Описание'
				dataIndex='description'
				defaultSortOrder='ascend'
				sorter={(a: TagInfo, b: TagInfo) =>
					(a.description ?? '').localeCompare(b.description ?? '')
				}
			/>
			{!hideSource && (
				<Column<TagInfo>
					title='Источник'
					dataIndex='sourceId'
					width='18em'
					defaultSortOrder='ascend'
					sorter={(a: TagInfo, b: TagInfo) =>
						(a.sourceName ?? String(a.sourceId)).localeCompare(
							b.sourceName ?? String(b.sourceId),
						)
					}
					render={(_, record) => (
						<SourceEl
							id={record.sourceId ?? 0}
							name={record.sourceName ?? '?'}
						/>
					)}
				/>
			)}
			{!hideValue && (
				<Column<TagInfo>
					title='Значение'
					defaultSortOrder='ascend'
					width='12em'
					sorter={(a: TagInfo, b: TagInfo) =>
						compareValues(values[a.id]?.value, values[b.id]?.value)
					}
					render={(_, record: TagInfo) => {
						const value = values[record.id]
						return (
							<TagCompactValue
								value={value?.value}
								type={record.type}
								quality={value?.quality}
							/>
						)
					}}
				/>
			)}
		</Table>
	)
}

export default TagsTable
