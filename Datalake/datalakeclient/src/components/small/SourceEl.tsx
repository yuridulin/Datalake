import { TagSource } from "../../@types/Source"
import { Button } from "antd"
import { NavLink } from "react-router-dom"
import { ManualId, CalculatedId } from "../../@types/enums/CustomSourcesIdentity"

type HeaderProps = {
	sources: TagSource[]
	id: number
}

export default function SourceEl({ sources, id }: HeaderProps) {
	if (id === ManualId) {
		return <NavLink to={`/tags/manual/`}>
			<Button>Мануальный</Button>
		</NavLink>
	}
	else if (id === CalculatedId) {
		return <NavLink to={`/tags/calc/`}>
			<Button>Вычисляемый</Button>
		</NavLink>
	}
	else {
		let finded = sources.filter(x => x.Id === id)
		if (finded.length > 0) {
			return <NavLink to={`/sources/${finded[0].Id}`}>
				<Button>{finded[0].Name}</Button>
			</NavLink>
		}
		else {
			return <span>?</span>
		}
	}
}