import { ValuesTagResponse } from '@/api/swagger/data-contracts'
import { BlockFlattenNestedTagInfo } from '@/app/pages/values/types/flattenedNestedTags'

export type TagValueWithInfo = ValuesTagResponse &
	BlockFlattenNestedTagInfo & {
		localName: string // Добавляем локальное имя
	}
