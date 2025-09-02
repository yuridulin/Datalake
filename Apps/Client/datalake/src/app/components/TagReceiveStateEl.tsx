import { TagReceiveState } from '@/generated/data-contracts'
import { timeMask } from '@/store/appStore'
import { theme } from 'antd'
import dayjs from 'dayjs'

interface TagReceiveStateProps {
	receiveState?: TagReceiveState
}

const TagReceiveStateEl = ({ receiveState }: TagReceiveStateProps) => {
	const { token } = theme.useToken()

	if (receiveState?.message)
		return (
			<span
				style={{ color: token.colorError }}
				title={`Последнее вычисление: ${dayjs(receiveState.date).format(timeMask)}`}
			>
				{receiveState.message}
			</span>
		)
	return <span style={{ color: token.colorSuccess }}>Без ошибок</span>
}

export default TagReceiveStateEl
