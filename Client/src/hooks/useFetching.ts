import { useState } from 'react'

export const useFetching = (callback: { (x?: any): Promise<void> }) => {
	const [isLoading, setIsLoading] = useState(false)
	const [error, setError] = useState('')

	const fetching = async (x?: any) => {
		try {
			setIsLoading(true)
			await callback(x)
		} catch (e) {
			console.log(e)
			setError((e as Error).message)
		} finally {
			setIsLoading(false)
		}
	}

	return [fetching, isLoading, error] as [
		(x?: any) => Promise<void>,
		boolean,
		string,
	]
}
