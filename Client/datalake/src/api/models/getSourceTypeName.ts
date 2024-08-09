import { SourceType } from '../swagger/data-contracts'

function getSourceTypeName(sourceType: SourceType) {
	switch (sourceType) {
		case SourceType.DatalakeCoreV1:
			return 'Datalake v1'
		case SourceType.Datalake:
			return 'Datalake v0'
		case SourceType.Inopc:
			return 'Inopc'
		case SourceType.Custom:
			return 'Служебный'
		default:
			return 'Не определён'
	}
}

export default getSourceTypeName
