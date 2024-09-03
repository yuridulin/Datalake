import { useCallback } from 'react'
import api from '../../api/swagger-api'
import { TagQuality } from '../../api/swagger/data-contracts'

const style: React.CSSProperties = {
	border: '1px solid gray',
	padding: '0 5px',
	minWidth: '1em',
}

export default function TagValueEl({
	value,
	allowEdit = false,
	guid,
}: {
	value?: string | number | boolean | null
	allowEdit?: boolean
	guid?: string
}) {
	const type = Object.prototype.toString.call(value)

	const tryEdit = useCallback(
		function () {
			if (!allowEdit) return
			const newValue = prompt('Введите новое значение', '')
			if (newValue !== null) {
				api.valuesWrite([
					{
						guid: guid,
						quality: TagQuality.GoodManualWrite,
						value: newValue,
					},
				])
			}
		},
		[allowEdit, guid],
	)

	switch (type) {
		case '[object Boolean]':
			return (
				<span style={{ color: '#1d39c4', ...style }}>
					<b>{value ? 'TRUE' : 'FALSE'}</b>
				</span>
			)
		case '[object Number]':
			return (
				<span style={{ color: '#d4380d', ...style }}>
					<b>{value ?? '?'}</b>
				</span>
			)
		case '[object String]':
			return (
				<span
					style={{ color: '#389e0d', ...style }}
					onDoubleClick={tryEdit}
				>
					<b>{value ?? '?'}</b>
				</span>
			)
		default:
			return <span>?</span>
	}
}
