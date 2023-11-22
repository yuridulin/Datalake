import { SourceType } from "./enums/SourceType"

export interface TagSource {
	Id: number
	Name: string
	Type: SourceType
	Address: string
}