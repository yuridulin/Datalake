import { TagHistoryUse } from "./enums/TagHistoryUse"
import { TagQuality } from "./enums/TagQuality"

export interface HistoryValue {
	Date: Date
	Value: string | number | boolean | undefined
	Quality: TagQuality
	Using: TagHistoryUse
}