import { TagQuality, TagSimpleInfo } from '@/generated/data-contracts'

export type ValueType = TagSimpleInfo & {
	value?: string | number | boolean | null
	quality: TagQuality
	date: string
	localName: React.ReactNode
}
