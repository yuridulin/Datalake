import { TagTypeDescription } from "./enums/TagTypeDescription"
import { TagQuality } from "./enums/TagQuality"
import { TagHistoryUse } from "./enums/TagHistoryUse"

export interface TagHistory {
	TagId: number
	Date: Date
	Text: string
	Number?: number
	Quality: TagQuality
	Type: TagType
	Using: TagHistoryUse
	TagName: string
	Value?: string | number| boolean
}