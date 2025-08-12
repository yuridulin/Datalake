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
		<NavLink to={routes.blocks.toViewBlock(block.id)}>
			<Button size='small' icon={<BlockIcon />}>
				{block.name}
			</Button>
		</NavLink>
	)
}

export default BlockButton
