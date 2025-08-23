export interface TransformedData {
	time: string
	dateString: string
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	[key: string]: any // Для динамических свойств tag1, tag2 и т.д.
}
