import { TagHistoryUse } from "./enums/TagHistoryUse"
import { TagQuality } from "./enums/TagQuality"
import { TagTypeDescription } from "./enums/TagTypeDescription"

export interface TagValue {
	TagId: number
	TagName: string
	Date: Date
	Quality: TagQuality
	Type: TagType
	Using: TagHistoryUse
	Value?: string | number| boolean
}