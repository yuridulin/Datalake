export default function TagValueEl({
	value,
}: {
	value?: string | number | boolean | undefined
}) {
	let type = Object.prototype.toString.call(value)

	switch (type) {
		case '[object Boolean]':
			return (
				<span style={{ color: '#1d39c4' }}>
					<b>{value ? 'TRUE' : 'FALSE'}</b>
				</span>
			)
		case '[object Number]':
			return (
				<span style={{ color: '#d4380d' }}>
					<b>{value}</b>
				</span>
			)
		case '[object String]':
			return (
				<span style={{ color: '#389e0d' }}>
					<b>{value}</b>
				</span>
			)
		default:
			return <span></span>
	}
}
