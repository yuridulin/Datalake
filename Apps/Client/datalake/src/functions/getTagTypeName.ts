import { TagType } from '@/generated/data-contracts'

export const TagTypeName: Record<TagType, string> = {
	[TagType.String]: 'строка',
	[TagType.Number]: 'число',
	[TagType.Boolean]: 'дискрет',
}
