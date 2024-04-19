export default function TagValueEl({
	value,
}: {
	value?: string | number | boolean
}) {
	let type = Object.prototype.toString.call(value)

	if (type === '[object Boolean]')
		return (
			<span style={{ color: '#1d39c4' }}>
				<b>{value ? 'TRUE' : 'FALSE'}</b>
			</span>
		)
	if (type === '[object Number]')
		return (
			<span style={{ color: '#d4380d' }}>
				<b>{value}</b>
			</span>
		)

	return (
		<span style={{ color: '#389e0d' }}>
			<b>{value}</b>
		</span>
	)
}
