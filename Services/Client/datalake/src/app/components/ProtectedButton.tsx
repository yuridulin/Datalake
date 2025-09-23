import { AccessType } from '@/generated/data-contracts'
import { Button, Tooltip } from 'antd'

interface ProtectedButtonProps {
	access: AccessType
	required: AccessType
	children: React.ReactNode
	onClick?: () => void
	type?: 'link' | 'text' | 'default' | 'primary' | 'dashed'
}

const ProtectedButton = ({ access, required, children, ...buttonProps }: ProtectedButtonProps) => {
	const hasAccess = access >= required

	const reason = hasAccess
		? undefined
		: required === AccessType.Admin
			? 'Необходим полный доступ'
			: required === AccessType.Manager
				? 'Необходим уровень доступа "Менеджер"'
				: required === AccessType.Editor
					? 'Необходим уровень доступа "Менеджер"'
					: required === AccessType.Viewer
						? 'Необходим уровень доступа "Менеджер"'
						: undefined

	return (
		<Tooltip title={!hasAccess ? reason : undefined}>
			<Button {...buttonProps} disabled={!hasAccess}>
				{children}
			</Button>
		</Tooltip>
	)
}

export default ProtectedButton
