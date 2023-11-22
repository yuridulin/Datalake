export enum TagHistoryUse {
	Initial = 0,
	Basic = 1,
	Aggregated = 2,
	Continuous = 3,
	Outdated = 100,
}

export function TagHistoryUseDescription(type: TagHistoryUse) {
	switch (type) {
		case TagHistoryUse.Initial: return 'начальное'
		case TagHistoryUse.Basic: return 'полученное'
		case TagHistoryUse.Aggregated: return 'вычисленное'
		case TagHistoryUse.Continuous: return 'протянутое'
		case TagHistoryUse.Outdated: return 'устаревшее'
		default: return '?'
	}
}