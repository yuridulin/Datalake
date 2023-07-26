import { Rel_Tag_Input } from "./Rel_Tag_Input"
import { TagType } from "./TagType"

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
	Inputs: Rel_Tag_Input[]
}