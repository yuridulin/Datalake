import { NavLink } from "react-router-dom"
import MyIcon from "../../components/myIcon/MyIcon"

export default function Tree() {

	return (
		<div className="tree">
			<div className="tree-item">
				<MyIcon icon="desktop_windows" />
				<NavLink to="/agents">Агенты</NavLink>
			</div>
			<div className="tree-item">
				<MyIcon icon="checklist_rtl" />
				<NavLink to="/presets">Назначения</NavLink>
			</div>
			<div className="tree-item">
				<MyIcon icon="tune" />
				<NavLink to="/filters">Фильтры</NavLink>
			</div>
		</div>
	)
}