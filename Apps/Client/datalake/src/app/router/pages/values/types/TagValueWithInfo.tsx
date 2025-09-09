import { ValuesTagResponse } from '@/generated/data-contracts'
import { BlockFlattenNestedTagInfo } from './flattenedNestedTags'

export type TagValueWithInfo = ValuesTagResponse &
	BlockFlattenNestedTagInfo & {
		localName: string // Добавляем локальное имя
	}
