import { SourceSimpleInfo } from '@/api/swagger/data-contracts'
import SourceIcon from '@/app/components/icons/SourceIcon'
import routes from '@/app/router/routes'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type SourceButtonProps = {
	source: SourceSimpleInfo
}

const SourceButton = ({ source }: SourceButtonProps) => {
	return (
		/*  hasAccess(sourceInfo.accessRule.accessType, AccessType.Viewer) ? ( */
		<NavLink to={routes.sources.toEditSource(source.id)}>
			<Button size='small' icon={<SourceIcon />}>
				{source.name}
			</Button>
		</NavLink>
	)
	{
		/*
	) : (
		<Button size='small' disabled icon={<UserIcon />}>
			Нет доступа
		</Button>
	) */
	}
}

export default SourceButton
