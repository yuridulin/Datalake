import { ValuesResponse } from '../swagger/data-contracts'

function getDictFromValuesResponseArray(valuesResponseArray: ValuesResponse[]) {
	return valuesResponseArray
		.flatMap((x) =>
			x.tags.map((t) => ({
				[t.guid]: !!t.values
					? t.values.length > 0
						? t.values[0].value ?? ''
						: ''
					: '',
			})),
		)
		.reduce((next, current) => ({ ...next, ...current }), {})
}

export default getDictFromValuesResponseArray
