import { LiveRequest } from "./LiveRequest"
import { AggFunc } from "./enums/AggFunc"

export interface HistoryRequest extends LiveRequest {
	Old?: Date
	Young?: Date
	Resolution?: number
	Func: AggFunc
}