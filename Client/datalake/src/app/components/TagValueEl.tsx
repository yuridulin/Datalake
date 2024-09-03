import { useCallback } from 'react'
import api from '../../api/swagger-api'
import { TagQuality, TagType } from '../../api/swagger/data-contracts'

const style: React.CSSProperties = {
	border: '1px solid #8080804d',
	borderRadius: '3px',
	padding: '3px 6px',
	minWidth: '1em',
}

export default function TagValueEl({
	value,
	type,
	allowEdit = false,
	guid,
}: {
	value?: string | number | boolean | null
	type: TagType
	allowEdit?: boolean
	guid?: string
}) {
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
	const valuer =
		type === TagType.Boolean ? (
			<span style={{ color: '#5273e0' }}>{value ? 'true' : 'false'}</span>
		) : type === TagType.Number ? (
			<span style={{ color: '#e87040' }}>{value ?? '?'}</span>
		) : type === TagType.String ? (
			<span style={{ color: '#6abe39' }}>{value ?? '?'}</span>
		) : (
			<span>?</span>
		)

	return (
		<span
			style={style}
			onDoubleClick={tryEdit}
			title={
				allowEdit ? 'Сделайте двойной клик для изменения значения' : ''
			}
		>
			{valuer}
		</span>
	)
}
