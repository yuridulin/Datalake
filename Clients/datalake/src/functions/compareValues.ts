import { ValueRecord } from '@/generated/data-contracts'
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

const compareRecords = (a: ValueRecord | null | undefined, b: ValueRecord | null | undefined): number => {
	// Обработка null/undefined
	if (a == null && b == null) return 0
	if (a == null) return 1 // null считается "больше", идёт в конец
	if (b == null) return -1

	// Определяем порядок типов
	const typeOrder = (record: ValueRecord): number => {
		if (record.boolean != null) return 0
		if (record.number != null) return 1
		if (record.text != null) return 2
		return 3 // все null
	}

	const typeA = typeOrder(a)
	const typeB = typeOrder(b)

	if (typeA !== typeB) {
		return typeA - typeB
	}

	// Сравнение по конкретному типу
	if (a.boolean != null && b.boolean != null) {
		return Number(a.boolean) - Number(b.boolean)
	}

	if (a.number != null && b.number != null) {
		return a.number - b.number
	}

	if (a.text != null && b.text != null) {
		return a.text.localeCompare(b.text)
	}

	// Если оба пустые (все поля null/undefined)
	return 0
}

const compareDateStrings = (a: string | null | undefined, b: string | null | undefined): number => {
	// Обработка null/undefined
	if (a == null && b == null) return 0
	if (a == null) return 1 // null в конец
	if (b == null) return -1

	// Попробуем распарсить как даты
	const timeA = Date.parse(a)
	const timeB = Date.parse(b)

	const validA = !isNaN(timeA)
	const validB = !isNaN(timeB)

	if (validA && validB) {
		return timeA - timeB
	}

	// Если хотя бы одна строка невалидна как дата → сравниваем как строки
	return a.localeCompare(b)
}

export { compareDateStrings, compareRecords }
