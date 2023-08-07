import { TagType } from "./enums/TagType"
import { TagQuality } from "./enums/TagQuality"
import { TagHistoryUse } from "./enums/TagHistoryUse"

export interface TagHistory {
	TagId: number
	Date: Date
	Text: string
	Number?: number
	Quality: keyof typeof TagQuality
	Type: keyof typeof TagType
	Using: keyof typeof TagHistoryUse
	TagName: string
	Value?: string | number| boolean
}