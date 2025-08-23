import UserGroupIcon from '@/app/components/icons/UserGroupIcon'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserGroupSimpleInfo } from '@/generated/data-contracts'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type UserGroupButtonProps = {
	group: UserGroupSimpleInfo
	check?: boolean
}

const UserGroupButton = ({ group, check = true }: UserGroupButtonProps) =>
	!check || hasAccess(group.accessRule.access, AccessType.Viewer) ? (
		<NavLink to={routes.userGroups.toViewUserGroup(group.guid)}>
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
