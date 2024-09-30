import { Button } from 'antd'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../api/models/customSource'

type HeaderProps = {
	id: number
	name: string
}

export default function SourceEl({ id, name }: HeaderProps) {
	if (id === CustomSource.Manual) {
		return (
			<NavLink to={`/tags/manual/`}>
				<Button size='small'>Мануальный</Button>
			</NavLink>
		)
	} else if (id === CustomSource.Calculated) {
		return (
			<NavLink to={`/tags/calc/`}>
				<Button size='small'>Вычисляемый</Button>
			</NavLink>
		)
	} else {
		return (
			<NavLink to={`/sources/${id}`}>
				<Button size='small'>{name}</Button>
			</NavLink>
		)
	}
}
