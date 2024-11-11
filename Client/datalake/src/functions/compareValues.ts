import { TagValue } from '../types/tagValue'

export default function compareValues(a: TagValue, b: TagValue): number {
	const typeOrder = (value: TagValue): number => {
		if (typeof value === 'boolean') return 0
		if (typeof value === 'number') return 1
		if (typeof value === 'string') return 2
		return 3
	}

	const typeA = typeOrder(a)
	const typeB = typeOrder(b)

	if (typeA !== typeB) {
		return typeA - typeB
	}

	if (typeof a === 'boolean' && typeof b === 'boolean') {
		return Number(a) - Number(b)
	}

	if (typeof a === 'number' && typeof b === 'number') {
		return a - b
	}

	if (typeof a === 'string' && typeof b === 'string') {
		return a.localeCompare(b)
	}

	return 0
}
