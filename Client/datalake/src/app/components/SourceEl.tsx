import { Button } from 'antd'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../types/customSource'
import routes from '../router/routes'

type HeaderProps = {
	id: number
	name: string
}

export default function SourceEl({ id, name }: HeaderProps) {
	if (id === CustomSource.Manual) {
		return (
			<NavLink to={routes.tags.manual}>
				<Button size='small'>Мануальный</Button>
			</NavLink>
		)
	} else if (id === CustomSource.Calculated) {
		return (
			<NavLink to={routes.tags.calc}>
				<Button size='small'>Вычисляемый</Button>
			</NavLink>
		)
	} else {
		return (
			<NavLink to={routes.sources.toEditSource(id)}>
				<Button size='small'>{name}</Button>
			</NavLink>
		)
	}
}
