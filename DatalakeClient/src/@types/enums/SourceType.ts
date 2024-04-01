export enum SourceType {
	Inopc = 0,
	Datalake = 1,
}

export function SourceTypeDescription(type: SourceType) {
	switch (type) {
		case SourceType.Inopc: return 'iNOPC Server'
		case SourceType.Datalake: return 'Datalake Node'
		default: return '?'
	}
}