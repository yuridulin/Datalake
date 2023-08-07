import { TagHistoryUse } from "./enums/TagHistoryUse"
import { TagQuality } from "./enums/TagQuality"
import { TagType } from "./enums/TagType"

export interface TagValue {
	TagId: number
	TagName: string
	Date: Date
	Quality: keyof typeof TagQuality
	Type: keyof typeof TagType
	Using: keyof typeof TagHistoryUse
	Value?: string | number| boolean
}