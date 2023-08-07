import { SourceType } from "./enums/SourceType"

export interface TagSource {
	Id: number
	Name: string
	Type: keyof typeof SourceType
	Address: string
}