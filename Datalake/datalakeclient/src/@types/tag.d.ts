import { TagInput } from "./tagInput"
import { TagType } from "./valueRange"

export interface Tag {
	Id: number
	Name: string
	Description: string
	Type: TagType
	SourceId: number
	SourceItem: string
	Source: string
	Interval: number

	IsScaling: boolean
	MinEU: number
	MaxEU: number
	MinRaw: number
	MaxRaw: number

	IsCalculating: boolean
	Formula: string
	Inputs: TagInput[]
}