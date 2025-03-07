import { SourceType } from '../api/swagger/data-contracts'

function getSourceTypeName(sourceType: SourceType) {
	switch (sourceType) {
		case SourceType.DatalakeV2:
			return 'Datalake v2'
		case SourceType.Datalake:
			return 'Datalake v1'
		case SourceType.Inopc:
			return 'Inopc'
		case SourceType.System:
			return 'Служебный'
		default:
			return 'Не определён'
	}
}

export default getSourceTypeName
