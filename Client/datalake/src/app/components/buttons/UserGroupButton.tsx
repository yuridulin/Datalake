import { AccessType, UserGroupSimpleInfo } from '@/api/swagger/data-contracts'
import UserGroupIcon from '@/app/components/icons/UserGroupIcon'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type UserGroupButtonProps = {
	group: UserGroupSimpleInfo
}

const UserGroupButton = ({ group }: UserGroupButtonProps) =>
	hasAccess(group.accessRule.accessType, AccessType.Viewer) ? (
		<NavLink to={routes.userGroups.toUserGroup(group.guid)}>
			<Button size='small' icon={<UserGroupIcon />}>
				{group.name}
			</Button>
		</NavLink>
	) : (
		<Button size='small' disabled icon={<UserGroupIcon />}>
			Нет доступа
		</Button>
	)

export default UserGroupButton
