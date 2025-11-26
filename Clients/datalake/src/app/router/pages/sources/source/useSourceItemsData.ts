import { LoadStatus } from '@/app/components/loaders/loaderTypes'
import { SourceWithSettingsAndTagsInfo, TagType } from '@/generated/data-contracts'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { useCallback, useEffect, useRef, useState } from 'react'
import { SourceEntryInfo, SourceTagInfo } from './SourceItems.types'
import { mergeEntries, toSourceTagInfo } from './SourceItems.utils'

export const useSourceItemsData = (source: SourceWithSettingsAndTagsInfo) => {
	const store = useAppStore()
	const [items, setItems] = useState([] as SourceEntryInfo[])
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])
	const [err, setErr] = useState(true)
	const [created, setCreated] = useState(null as SourceTagInfo | null)
	const [status, setStatus] = useState<LoadStatus>('default')
	const hasLoadedRef = useRef(false)
	const lastSourceIdRef = useRef<number | undefined>(source.id)

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

	return {
		items,
		searchedItems,
		setSearchedItems,
		err,
		created,
		setCreated,
		status,
		reload,
		reloadDone,
		createTag,
		deleteTag,
	}
}
