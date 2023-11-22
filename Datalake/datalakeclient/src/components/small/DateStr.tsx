export default function DateStr({ date }: { date: Date }) {

	if (!date) return <>?</>
	if (Object.prototype.toString.call(date) !== '[object Date]') date = new Date(date)

	function appendZero(n: number) {
		return n < 10 ? ('0' + n) : ('' + n)
	}

	function appendTwoZeros(n: number) {
		return n < 100 ? ('0' + n) : (n < 10 ? ('00' + n) : ('' + n))
	}

	return <>{appendZero(date.getDate())}.{appendZero(date.getMonth() + 1)}.{date.getFullYear()} {appendZero(date.getHours())}:{appendZero(date.getMinutes())}:{appendZero(date.getSeconds())}.{appendTwoZeros(date.getMilliseconds())}</>
}