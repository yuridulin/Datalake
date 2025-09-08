import { useTitle } from 'react-use'

const useDatalakeTitle = (...titleParts: (string | undefined)[]) => {
	useTitle(['Datalake', ...titleParts].filter(Boolean).join(' | '))
}

export default useDatalakeTitle
