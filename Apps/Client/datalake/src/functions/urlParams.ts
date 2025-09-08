import { timeMask } from '@/store/appStore'
import dayjs, { Dayjs } from 'dayjs'

export const TimeModes = {
	LIVE: 'live',
	EXACT: 'exact',
	OLD_YOUNG: 'old-young',
} as const

export type TimeMode = (typeof TimeModes)[keyof typeof TimeModes]

// Константы для имен параметров URL
export const URL_PARAMS = {
	TAGS: 'T',
	VIEWER_MODE: 'V-M',
	VIEWER_RESOLUTION: 'V-R',
	VIEWER_EXACT: 'V-E',
	VIEWER_OLD: 'V-O',
	VIEWER_YOUNG: 'V-P',
	WRITER_DATE: 'W-D',
} as const

// Сериализация выбранных тегов
export const serializeTags = (relations: number[], tagMapping: Record<number, { id: number }>): string => {
	return relations
		.map((relationId) => {
			const tagInfo = tagMapping[relationId]
			return tagInfo ? `${tagInfo.id}.${relationId}` : ''
		})
		.filter(Boolean)
		.join('~')
}

// Извлечение пар "тег-связь" из строки
export const deserializeTags = (param: string | null): Array<{ tagId: number; relationId: number }> => {
	if (!param) return []

	return param
		.split('~')
		.map((pair) => {
			const [tagId, relationId] = pair.split('.').map(Number)
			return { tagId, relationId }
		})
		.filter(({ tagId, relationId }) => !isNaN(tagId) && !isNaN(relationId))
}

// Сериализация даты в строку
export const serializeDate = (date: Dayjs | null): string | null => {
	return date ? date.format(timeMask) : null
}

// Извлечение даты из строки
export const deserializeDate = (dateString: string | null): Dayjs | null => {
	return dateString ? dayjs(dateString, timeMask) : null
}

// Интерфейс для параметров просмотра значений
export interface ViewerParams {
	mode: TimeMode
	resolution?: number
	exact?: Dayjs | null
	old?: Dayjs | null
	young?: Dayjs | null
}

// Получение параметров просмотра значений из URLSearchParams
export const getViewerParams = (searchParams: URLSearchParams): ViewerParams => {
	return {
		mode: (searchParams.get(URL_PARAMS.VIEWER_MODE) as TimeMode) || 'live',
		resolution: Number(searchParams.get(URL_PARAMS.VIEWER_RESOLUTION)) || undefined,
		exact: deserializeDate(searchParams.get(URL_PARAMS.VIEWER_EXACT)),
		old: deserializeDate(searchParams.get(URL_PARAMS.VIEWER_OLD)),
		young: deserializeDate(searchParams.get(URL_PARAMS.VIEWER_YOUNG)),
	}
}

// Установка параметров просмотра значений в URLSearchParams
export const setViewerParams = (searchParams: URLSearchParams, params: ViewerParams): void => {
	if (!params.mode) {
		console.warn('Режим чтения данных не получен!')
		return
	}

	searchParams.set(URL_PARAMS.VIEWER_MODE, params.mode)

	if (params.mode === 'old-young') {
		if (params.resolution) {
			searchParams.set(URL_PARAMS.VIEWER_RESOLUTION, params.resolution.toString())
		}
		if (params.old) {
			searchParams.set(URL_PARAMS.VIEWER_OLD, serializeDate(params.old)!)
		}
		if (params.young) {
			searchParams.set(URL_PARAMS.VIEWER_YOUNG, serializeDate(params.young)!)
		}
	} else {
		searchParams.delete(URL_PARAMS.VIEWER_YOUNG)
		searchParams.delete(URL_PARAMS.VIEWER_OLD)
		searchParams.delete(URL_PARAMS.VIEWER_RESOLUTION)
	}

	if (params.mode === 'exact') {
		if (params.exact) {
			searchParams.set(URL_PARAMS.VIEWER_EXACT, serializeDate(params.exact)!)
		}
	} else {
		searchParams.delete(URL_PARAMS.VIEWER_EXACT)
	}
}

// Получение параметров записи значений из URLSearchParams
export interface WriterParams {
	date?: Dayjs | null
	tags?: Array<{ tagId: number; relationId: number }>
}

export const getWriterParams = (searchParams: URLSearchParams): WriterParams => {
	return {
		date: deserializeDate(searchParams.get(URL_PARAMS.WRITER_DATE)),
		tags: deserializeTags(searchParams.get(URL_PARAMS.TAGS)),
	}
}

// Установка параметров записи значений в URLSearchParams
export const setWriterParams = (searchParams: URLSearchParams, params: WriterParams): void => {
	if (params.date !== undefined) {
		if (params.date) {
			searchParams.set(URL_PARAMS.WRITER_DATE, serializeDate(params.date)!)
		} else {
			searchParams.delete(URL_PARAMS.WRITER_DATE)
		}
	}

	if (params.tags !== undefined) {
		if (params.tags && params.tags.length > 0) {
			const serializedTags = params.tags.map((tag) => `${tag.tagId}.${tag.relationId}`).join('~')
			searchParams.set(URL_PARAMS.TAGS, serializedTags)
		} else {
			searchParams.delete(URL_PARAMS.TAGS)
		}
	}
}
