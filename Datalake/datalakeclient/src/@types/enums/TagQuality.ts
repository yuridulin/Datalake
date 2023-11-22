export enum TagQuality {
	Bad = 0,
	Bad_NoConnect = 4,
	Bad_NoValues = 8,
	Bad_ManualWrite = 26,
	Good = 192,
	Good_ManualWrite = 216,
	Unknown = -1,
}

export function TagQualityDescription(type: TagQuality) {
	switch (type) {
		case TagQuality.Bad: return 'недостоверно'
		case TagQuality.Bad_NoConnect: return 'нет связи'
		case TagQuality.Bad_NoValues: return 'нет значения'
		case TagQuality.Bad_ManualWrite: return 'недостоверно, ручной ввод'
		case TagQuality.Good: return 'достоверно'
		case TagQuality.Good_ManualWrite: return 'достоверно, ручной ввод'
		case TagQuality.Unknown: return 'неизвестно'
		default: return '?'
	}
}