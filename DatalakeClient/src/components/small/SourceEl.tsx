import { Button } from 'antd'
import { NavLink } from 'react-router-dom'
import { CustomSource } from '../../etc/customSource'

type HeaderProps = {
	id: number
	name: string
}

export default function SourceEl({ id, name }: HeaderProps) {
	if (id === CustomSource.Manual) {
		return (
			<NavLink to={`/tags/manual/`}>
				<Button>Мануальный</Button>
			</NavLink>
		)
	} else if (id === CustomSource.Calculated) {
		return (
			<NavLink to={`/tags/calc/`}>
				<Button>Вычисляемый</Button>
			</NavLink>
		)
	} else {
		return (
			<NavLink to={`/sources/${id}`}>
				<Button>{name}</Button>
			</NavLink>
		)
	}
}
