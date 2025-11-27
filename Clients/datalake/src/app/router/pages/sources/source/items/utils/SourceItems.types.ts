import { SourceItemInfo, TagSimpleInfo } from '@/generated/data-contracts'

// Расширяем SourceTagInfo из API до TagSimpleInfo для совместимости с компонентами
export type SourceTagInfo = TagSimpleInfo & { item?: string | null }

export type SourceEntryInfo = {
	itemInfo?: SourceItemInfo
	tagInfo?: SourceTagInfo
	isTagInUse?: string
}

export interface GroupedEntry {
	path: string
	itemInfo?: SourceItemInfo
	tagInfoArray: SourceTagInfo[]
}

export interface TreeNodeData {
	key: string
	title: React.ReactNode
	children?: TreeNodeData[]
	isLeaf: boolean
	path: string
	group?: GroupedEntry
	countLeaves: number
	countTags: number
}

export type ViewModeState = 'table' | 'tree'
