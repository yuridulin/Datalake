export default function compareValues<T>(
	a: T | undefined,
	b: T | undefined,
): number {
	if (a === b) {
		return 0
	}
	if (a === undefined) {
		return -1
	}
	if (b === undefined) {
		return 1
	}

	if (typeof a === 'string' && typeof b === 'string') {
		return a.localeCompare(b)
	}

	if (typeof a === 'number' && typeof b === 'number') {
		return a - b
	}

	if (typeof a === 'boolean' && typeof b === 'boolean') {
		return a === b ? 0 : a ? 1 : -1
	}

	return 0
}
