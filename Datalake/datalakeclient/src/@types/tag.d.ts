import { TagType } from "./valueRange"

export interface Tag {
	Id: number
	Name: string
	Type: TagType
	Description: string
	SourceId: number
	SourceItem: string
	Source: string
	Interval: number
}