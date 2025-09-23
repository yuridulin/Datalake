import { serializeDate } from '@/functions/dateHandle'
import { TagReceiveState } from '@/generated/data-contracts'
import { theme } from 'antd'

interface TagReceiveStateProps {
	receiveState?: TagReceiveState
}

const TagReceiveStateEl = ({ receiveState }: TagReceiveStateProps) => {
	const { token } = theme.useToken()

	if (receiveState?.message)
		return (
			<span style={{ color: token.colorError }} title={`Последнее вычисление: ${serializeDate(receiveState.date)}`}>
				{receiveState.message}
			</span>
		)
	return <span style={{ color: token.colorSuccess }}>Без ошибок</span>
}

export default TagReceiveStateEl
