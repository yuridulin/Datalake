export enum TagType {
	String = 0,
	Number = 1,
	Boolean = 2,
}

export function TagTypeDescription(type: TagType) {
	switch (type) {
		case TagType.String: return 'строка'
		case TagType.Number: return 'число'
		case TagType.Boolean: return 'дискрет'
		default: return '?'
	}
}