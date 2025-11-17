import { theme } from 'antd'

interface TagReceiveStateProps {
	receiveState: string | undefined
}

const TagReceiveStateEl = ({ receiveState }: TagReceiveStateProps) => {
	const { token } = theme.useToken()

	if (receiveState) return <span style={{ color: token.colorError }}>{receiveState}</span>
	return <span style={{ color: token.colorSuccess }}>Без ошибок</span>
}

export default TagReceiveStateEl
