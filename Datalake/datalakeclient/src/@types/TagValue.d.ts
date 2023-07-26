export interface TagValue {
	TagId: number
	TagName: string
	Date: Date
	Quality: 0 | 4 | 192 | 216 | -1
	Type: 0 | 1 | 2 | 3
	Using: 0 | 1
	Value?: string | number| boolean
}