import { ValueRecord, ValuesResponse } from '../api/swagger/data-contracts'

function getDictFromValuesResponseArray(valuesResponseArray: ValuesResponse[]): { [key: number]: ValueRecord | null } {
	return valuesResponseArray
		.flatMap((x) =>
			x.tags.map((t) => ({
				[t.guid]: t.values ? (t.values.length > 0 ? t.values[0] : null) : null,
			})),
		)
		.reduce((next, current) => ({ ...next, ...current }), {})
}

export default getDictFromValuesResponseArray
