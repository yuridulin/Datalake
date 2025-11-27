import CreatedTagLinker from '@/app/components/CreatedTagsLinker'
import StatusLoader from '@/app/components/loaders/StatusLoader'
import { useSourceItemsSearch } from '@/app/router/pages/sources/source/items/utils/useSourceItemsSearch'
import { SourceUpdateRequest, SourceWithSettingsAndTagsInfo } from '@/generated/data-contracts'
import { Alert } from 'antd'
import { useLocalStorage } from 'react-use'
import { SourceItemsTable } from './SourceItemsTable'
import { SourceItemsToolbar } from './SourceItemsToolbar'
import { SourceItemsTree } from './SourceItemsTree'
import { ViewModeState } from './utils/SourceItems.types'
import { groupEntries } from './utils/SourceItems.utils'
import { useSourceItemsData } from './utils/useSourceItemsData'
type SourceItemsProps = {
	source: SourceWithSettingsAndTagsInfo
	request: SourceUpdateRequest
}

const localStorageKey = 'sourceItemsViewMode'

const SourceItems = ({ source, request }: SourceItemsProps) => {
	const [paginationConfig, setPaginationConfig] = useLocalStorage('sourceItems-' + source.id, {
		pageSize: 10,
		current: 1,
	})
	const [viewMode, setViewMode] = useLocalStorage<ViewModeState>(localStorageKey, 'table')

	const { items, err, created, setCreated, status, reload, reloadDone, createTag, deleteTag } =
		useSourceItemsData(source)

	const { search, searchedItems: finalSearchedItems, onSearchChange } = useSourceItemsSearch(items)

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
							<SourceItemsToolbar
								search={search}
								onSearchChange={onSearchChange}
								viewMode={viewMode ?? 'table'}
								onViewModeChange={setViewMode}
								onReload={reload}
							/>

							{(viewMode ?? 'table') === 'table' ? (
								<SourceItemsTable
									items={finalSearchedItems}
									paginationConfig={paginationConfig ?? { pageSize: 10, current: 1 }}
									onPaginationChange={(page, pageSize) => {
										setPaginationConfig({ current: page, pageSize: pageSize || 10 })
									}}
									onCreateTag={createTag}
									onDeleteTag={deleteTag}
								/>
							) : (
								<SourceItemsTree
									groups={groupEntries(finalSearchedItems)}
									onCreateTag={createTag}
									onDeleteTag={deleteTag}
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
