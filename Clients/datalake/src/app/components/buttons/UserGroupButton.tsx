import UserGroupIcon from '@/app/components/icons/UserGroupIcon'
import routes from '@/app/router/routes'
import hasAccess from '@/functions/hasAccess'
import { AccessType, UserGroupSimpleInfo } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { Button } from 'antd'
import { useMemo } from 'react'
import { NavLink } from 'react-router-dom'

type UserGroupButtonProps = {
	group: UserGroupSimpleInfo & { accessRule?: { access: AccessType } }
	check?: boolean
}

const UserGroupButton = ({ group, check = true }: UserGroupButtonProps) => {
	const store = useAppStore()
	const hasGroupAccess = useMemo(() => {
		if (!check) return true
		if (group.accessRule) {
			return hasAccess(group.accessRule.access, AccessType.Viewer)
		}
		return store.hasAccessToGroup(AccessType.Viewer, group.guid)
	}, [check, group.accessRule, group.guid, store])

	return hasGroupAccess ? (
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
}

export default UserGroupButton
