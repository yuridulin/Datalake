import { TagSimpleInfo } from '@/api/swagger/data-contracts'
import TagIcon from '@/app/components/icons/TagIcon'
import routes from '@/app/router/routes'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type TagButtonProps = {
	tag: TagSimpleInfo
}

const TagButton = ({ tag }: TagButtonProps) => {
	return (
		/* hasAccess(tag.accessRule.accessType, AccessType.Viewer) ?*/ <NavLink
			to={routes.tags.toTagForm(tag.guid)}
		>
			<Button size='small' icon={<TagIcon />}>
				{tag.name}
			</Button>
		</NavLink>
	) /* : (
		<Button size='small' disabled icon={<TagIcon />}>
			Нет доступа
		</Button>
	) */
}

export default TagButton
