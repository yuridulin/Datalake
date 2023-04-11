import Value from './value'

export interface ValueRange {
	TagName: string
	TagType: TagType
	Values: Value[]
}

export enum TagType {
	String = 0,
	Number = 1,
	Boolean = 2
}