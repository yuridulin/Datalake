import { BlockSimpleInfo } from '@/api/swagger/data-contracts'
import BlockIcon from '@/app/components/icons/BlockIcon'
import routes from '@/app/router/routes'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type BlockButtonProps = {
	block: BlockSimpleInfo
}

const BlockButton = ({ block }: BlockButtonProps) => {
	return (
		/* hasAccess(tag.accessRule.accessType, AccessType.Viewer) ?*/ <NavLink
			to={routes.blocks.toViewBlock(block.id)}
		>
			<Button size='small' icon={<BlockIcon />}>
				{block.name}
			</Button>
		</NavLink>
	) /* : (
		<Button size='small' disabled icon={<TagIcon />}>
			Нет доступа
		</Button>
	) */
}

export default BlockButton
