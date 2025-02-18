import { TagSimpleInfo } from '@/api/swagger/data-contracts'
import TagIcon from '@/app/components/icons/TagIcon'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import routes from '@/app/router/routes'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type TagButtonProps = {
	tag: TagSimpleInfo
}

const TagButton = ({ tag }: TagButtonProps) => {
	return (
		<NavLink to={routes.tags.toTagForm(tag.guid)}>
			<Button size='small' icon={<TagIcon type={tag.sourceType} />}>
				{tag.name}
				<TagFrequencyEl frequency={tag.frequency} />
			</Button>
		</NavLink>
	)
}

export default TagButton
