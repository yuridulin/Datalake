import { SourceType } from '@/api/swagger/data-contracts'
import SourceButton from '@/app/components/buttons/SourceButton'
import { Button } from 'antd'
import { NavLink } from 'react-router-dom'
import routes from '../router/routes'

type HeaderProps = {
	id: number
	name: string
}

export default function SourceEl({ id, name }: HeaderProps) {
	if (id === SourceType.Manual) {
		return (
			<NavLink to={routes.tags.manual}>
				<Button size='small'>Мануальный</Button>
			</NavLink>
		)
	} else if (id === SourceType.Calculated) {
		return (
			<NavLink to={routes.tags.calc}>
				<Button size='small'>Вычисляемый</Button>
			</NavLink>
		)
	} else {
		return <SourceButton source={{ id, name }} />
	}
}
