export enum BlockTagType {
	Input = 0,
	Output = 1,
	Relation = 2,
}

export function BlockTagTypeDescription(type: BlockTagType) {
	switch (type) {
		case BlockTagType.Input: return 'вход'
		case BlockTagType.Output: return 'выход'
		case BlockTagType.Relation: return 'связанный'
		default: return '?'
	}
}