import { BlockFlattenNestedTagInfo } from '@/app/components/tagTreeSelect/treeSelectShared'
import { ValuesTagResponse } from '@/generated/data-contracts'

export type TagValueWithInfo = ValuesTagResponse &
	BlockFlattenNestedTagInfo & {
		localName: string // Добавляем локальное имя
	}
