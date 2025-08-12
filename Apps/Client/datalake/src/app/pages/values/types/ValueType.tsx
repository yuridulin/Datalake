import { TagQuality, TagSimpleInfo } from '@/api/swagger/data-contracts'

export type ValueType = TagSimpleInfo & {
	value?: string | number | boolean | null
	quality: TagQuality
	date: string
	localName: React.ReactNode
}
