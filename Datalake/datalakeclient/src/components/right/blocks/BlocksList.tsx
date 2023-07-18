import { Button } from "antd";
import { NavLink, Navigate } from "react-router-dom";
import Header from "../../small/Header";
import { useEffect, useState } from "react";
import { BlockType } from "../../../@types/BlockType";
import { useFetching } from "../../../hooks/useFetching";
import axios from "axios";

export default function Dashboard() {

	const [ blocks, setBlocks ] = useState([] as BlockType[])

	const [ load, , error ] = useFetching(async() => {
		let res = await axios.post('blocks/list')
		setBlocks(res.data)
	})

	const [ createBlock ] = useFetching(async () => {
		let res = await axios.post('blocks/create', { ParentId: 0 })
		if (res.data.Done) load()
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])

	return (
		error
		? <Navigate to="" />
		: <>
			<Header
				right={<Button onClick={createBlock}>Добавить блок</Button>}
			>Блоки верхнего уровня</Header>
			{blocks.length > 0
			? <div className="table">
				<div className="table-header">
					<span>Имя</span>
					<span>Описание</span>
					<span>Кол-во тегов</span>
				</div>
				{blocks.map(x =>
					<NavLink className="table-row" to={'/blocks/view/' + x.Id} key={x.Id}>
						<span>{x.Name}</span>
						<span>{x.Description}</span>
						<span>{x.Children.length}</span>
					</NavLink>
				)}
			</div>
			: <div>не определено ни одного блока</div>}
		</>)
}