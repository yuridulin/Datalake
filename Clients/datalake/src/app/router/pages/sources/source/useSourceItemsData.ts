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
						// Новая модель: каждый элемент массива - одно использование с одним request
						usage = (usageResponse.data ?? []).reduce(
							(acc, item) => {
								if (item.tagId !== null && item.tagId !== undefined && item.request) {
									const tagIdKey = String(item.tagId)
									if (!acc[tagIdKey]) {
										acc[tagIdKey] = {}
									}
									// request - это ключ, date - значение
									if (item.date) {
										acc[tagIdKey][item.request] = item.date
									}
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
		try {
			const createdTag = await store.tagsStore.createTag({
				name: '',
				tagType: tagType,
				sourceId: source.id,
				sourceItem: item,
			})
			if (createdTag) {
				setCreated(createdTag)
				const newTag: SourceTagInfo = {
					...createdTag,
					item: item,
				}
				setItems((prev) => prev.map((x) => (x.itemInfo?.path === item ? { ...x, tagInfo: newTag } : x)))
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create tag'), {
				component: 'useSourceItemsData',
				action: 'createTag',
			})
		}
	}

	const deleteTag = async (tagId: number) => {
		try {
			await store.tagsStore.deleteTag(tagId)
			reload()
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete tag'), {
				component: 'useSourceItemsData',
				action: 'deleteTag',
			})
		}
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
