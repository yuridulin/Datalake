import { serializeDate } from '@/functions/dateHandle'
import { Dayjs } from 'dayjs'

export const TimeModes = {
	LIVE: 'live',
	EXACT: 'exact',
	OLD_YOUNG: 'old-young',
} as const

export type TimeMode = (typeof TimeModes)[keyof typeof TimeModes]

export const SELECTED_SEPARATOR: string = '~'
export const RELATION_TAG_SEPARATOR: string = '|'

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

// Интерфейс для параметров просмотра значений
export interface ViewerParams {
	mode: TimeMode
	resolution?: number
	exact?: Dayjs | null
	old?: Dayjs | null
	young?: Dayjs | null
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
