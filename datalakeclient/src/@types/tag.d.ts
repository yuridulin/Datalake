import { TagType } from "./valueRange"

export interface Tag {
	TagName: string
	TagType: TagType
	Description: string
	SourceId: number
	SourceItem: string
	Source: string
	Interval: number
}