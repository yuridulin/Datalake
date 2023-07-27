export default function TagValueElement({ value }: { value?: string | number | boolean }) {
	
	let type = Object.prototype.toString.call(value)

	if (type === '[object Boolean]') return (<div style={{ color: '#1d39c4' }}><b>{value ? 'TRUE' : 'FALSE'}</b></div>)
	if (type === '[object Number]') return (<div style={{ color: '#d4380d' }}><b>{value}</b></div>)

	return (
		<div style={{ color: '#389e0d' }}><b>{value}</b></div>
	)
}