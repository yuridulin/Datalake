export default function TagValueElement({ value }: { value?: string | number | boolean }) {
	
	let type = Object.prototype.toString.call(value)

	if (type === '[object Boolean]') return (<div style={{ color: '#1d39c4' }}>{value ? 'TRUE' : 'FALSE'}</div>)
	if (type === '[object Number]') return (<div style={{ color: '#d4380d' }}>{value}</div>)

	return (
		<div style={{ color: '#389e0d' }}>{value}</div>
	)
}