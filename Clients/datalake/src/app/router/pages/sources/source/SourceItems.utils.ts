import compareValues from '@/functions/compareValues'
import { SourceTagInfo as ApiSourceTagInfo, SourceItemInfo } from '@/generated/data-contracts'
import dayjs from 'dayjs'
import { SourceEntryInfo, SourceTagInfo } from './SourceItems.types'

// Преобразует SourceTagInfo из API в полный SourceTagInfo с расширенными полями TagSimpleInfo
// Использует значения из source для недостающих полей
export const toSourceTagInfo = (tag: ApiSourceTagInfo, sourceId: number, sourceType: number): SourceTagInfo => {
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

export const getLastUsage = (usage?: Record<string, string>) => {
	if (!usage) return undefined
	return Object.values(usage).reduce<string | undefined>((latest, current) => {
		if (!current) return latest
		if (!latest) return current
		return dayjs(current).isAfter(dayjs(latest)) ? current : latest
	}, undefined)
}

export const mergeEntries = (
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

export function groupEntries(items: SourceEntryInfo[]) {
	const map = items.reduce<Record<string, import('./SourceItems.types').GroupedEntry>>((acc, x) => {
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

// Функция для форматирования чисел
export const formatCount = (count: number) => (count > 0 ? `: ${count}` : ' нет')
