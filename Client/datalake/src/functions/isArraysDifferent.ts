export default function isArraysDifferent<T>(
	array1: Array<T>,
	array2: Array<T>,
) {
	const keys1 = array1
		.map((x) => String(x))
		.sort((a, b) => a.localeCompare(b))
	const keys2 = array2
		.map((x) => String(x))
		.sort((a, b) => a.localeCompare(b))

	if (keys1.length != keys2.length) return true
	for (let i = 0; i < keys1.length; i++)
		if (keys1[i] !== keys2[i]) return true
	return false
}
