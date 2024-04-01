import { LogType } from "./enums/LogType"

export interface Log {
	Date: Date
	Module: string
	Message: string
	Type: LogType
}