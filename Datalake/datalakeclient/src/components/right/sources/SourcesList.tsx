import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { TagSource } from "../../../@types/Source"
import { NavLink, Navigate } from "react-router-dom"
import { Button } from "antd"
import axios from "axios"
import Header from "../../small/Header"

export default function SourcesList() {

	const [ list, setList ] = useState([] as TagSource[])

	const [ loadList,, error ] = useFetching(async () => {
		const res = await axios.get('sources/list')
		setList(await res.data)
	})

	const createSource = () => {
		axios.post('sources/create')
			.then(() => loadList())
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { loadList() }, [])

	return (
		error
		? <Navigate to="/offline" />
		: <>
			<Header
				right={<Button onClick={createSource}>Добавить источник</Button>}
			>
				Зарегистрированные источники данных
			</Header>
			{list.length === 0 
			? <div><i>Не определено ни одного источника</i></div>
			: <div className="table">
				<div className="table-header">
					<span>Имя</span>
					<span>Адрес</span>
				</div>
				{list.map(x => 
					<NavLink className="table-row" to={'/sources/' + x.Id} key={x.Id}>
						<span>{x.Name}</span>
						<span>{x.Address}</span>
					</NavLink>
				)}
			</div>}
		</>
	)
}