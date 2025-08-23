import { BlockFlattenNestedTagInfo } from '@/app/pages/values/types/flattenedNestedTags'
import { ValuesTagResponse } from '@/generated/data-contracts'

export type TagValueWithInfo = ValuesTagResponse &
	BlockFlattenNestedTagInfo & {
		localName: string // Добавляем локальное имя
	}
