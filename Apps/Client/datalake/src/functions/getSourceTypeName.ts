import { SourceType } from '../generated/data-contracts'

function getSourceTypeName(sourceType: SourceType) {
	switch (sourceType) {
		case SourceType.Datalake:
			return 'Datalake'
		case SourceType.Inopc:
			return 'Inopc'
		case SourceType.System:
			return 'Служебный'
		case SourceType.Aggregated:
			return 'Агрегатор'
		case SourceType.Manual:
			return 'Ручной ввод'
		case SourceType.Calculated:
			return 'Вычислитель'
		default:
			return 'Не определён'
	}
}

export default getSourceTypeName
