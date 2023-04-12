export default function TagValue({ value }: { value: string | number | boolean }) {
	
	let type = Object.prototype.toString.call(value)

	if (type === '[object Boolean]') return (<div>{value ? 'true' : 'false'}</div>)

	return (
		<div>{value}</div>
	)
}