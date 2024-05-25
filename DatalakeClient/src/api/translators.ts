import { SourceType } from './swagger/data-contracts'

function sourceTypeName(sourceType: SourceType) {
	switch (sourceType) {
		case SourceType.Datalake:
			return 'Datalake'
		case SourceType.Inopc:
			return 'Inopc'
		case SourceType.Custom:
			return 'Служебный'
		default:
			return 'Не определён'
	}
}

export { sourceTypeName }
