import { HistoryValue } from "./HistoryValue"
import { AggFunc } from "./enums/AggFunc"
import { TagType } from "./enums/TagType"

export interface HistoryResponse {
	Id: number
	TagName: string
	Type: TagType
	Func: AggFunc
	Values: HistoryValue[]
}