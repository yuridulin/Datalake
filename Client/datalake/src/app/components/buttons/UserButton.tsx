import { AccessType, UserSimpleInfo } from '@/api/swagger/data-contracts'
import UserIcon from '@/app/components/icons/UserIcon'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { Button, Space } from 'antd'
import { NavLink } from 'react-router-dom'

type UserGroupButtonProps = {
	userInfo: UserSimpleInfo
	check?: boolean
}

const UserButton = ({ userInfo, check = true }: UserGroupButtonProps) => {
	return !check || hasAccess(userInfo.accessRule.accessType, AccessType.Viewer) ? (
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
