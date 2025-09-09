import { BlockNestedTagInfo, BlockSimpleInfo } from '@/generated/data-contracts'

export type BlockFlattenNestedTagInfo = BlockNestedTagInfo & {
	localName: string
	parents: BlockSimpleInfo[]
}

export type FlattenedNestedTagsType = Record<number, BlockFlattenNestedTagInfo>
