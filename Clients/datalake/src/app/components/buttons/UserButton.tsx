import UserIcon from '@/app/components/icons/UserIcon'
import routes from '@/app/router/routes'
import { AccessType, UserSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button, Space } from 'antd'
import { useMemo } from 'react'
import { NavLink } from 'react-router-dom'

type UserGroupButtonProps = {
	userInfo: UserSimpleInfo
}

const UserButton = ({ userInfo }: UserGroupButtonProps) => {
	const store = useAppStore()
	const hasAccess = useMemo(() => store.hasGlobalAccess(AccessType.Manager), [store])

	return hasAccess ? (
		<NavLink to={routes.users.toUserView(userInfo.guid)}>
			<Button size='small' icon={<UserIcon />}>
				{userInfo.fullName}
			</Button>
		</NavLink>
	) : (
		<Space size='small'>
			<UserIcon />
			{userInfo.fullName}
		</Space>
	)
}

export default UserButton
