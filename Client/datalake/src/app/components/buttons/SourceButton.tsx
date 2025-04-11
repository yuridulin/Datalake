import { SourceSimpleInfo, SourceType } from '@/api/swagger/data-contracts'
import AggregatedSourceIcon from '@/app/components/icons/AggregatedSourceIcon'
import CalculatedSourceIcon from '@/app/components/icons/CalculatedSourceIcon'
import ManualSourceIcon from '@/app/components/icons/ManualSourceIcon'
import SourceIcon from '@/app/components/icons/SourceIcon'
import routes from '@/app/router/routes'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'

type SourceButtonProps = {
	source: SourceSimpleInfo
}

const SourceButton = ({ source }: SourceButtonProps) => {
	/* if (!hasAccess(source.accessRule.accessType, AccessType.Viewer)) {
		return <Button size='small' disabled icon={<UserIcon />}>
			Нет доступа
		</Button>
	} */
	if (source.id === SourceType.Manual) {
		return (
			<NavLink to={routes.tags.manual}>
				<Button size='small' icon={<ManualSourceIcon />}>
					Мануальный
				</Button>
			</NavLink>
		)
	} else if (source.id === SourceType.Calculated) {
		return (
			<NavLink to={routes.tags.calc}>
				<Button size='small' icon={<CalculatedSourceIcon />}>
					Вычисляемый
				</Button>
			</NavLink>
		)
	} else if (source.id === SourceType.Aggregated) {
		return (
			<NavLink to={routes.tags.aggregated}>
				<Button size='small' icon={<AggregatedSourceIcon />}>
					Агрегатный
				</Button>
			</NavLink>
		)
	} else {
		return (
			<NavLink to={routes.sources.toEditSource(source.id)}>
				<Button size='small' icon={<SourceIcon />}>
					{source.name}
				</Button>
			</NavLink>
		)
	}
}

export default SourceButton
