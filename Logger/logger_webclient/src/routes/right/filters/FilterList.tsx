import axios from "axios";
import { useEffect, useState } from "react";
import { FilterType } from "../../../@types/filter";
import Header from "../../../components/header/Header";
import { Button } from "antd";
import ViewTable from "../../../components/viewTable/ViewTable";
import { Link } from "react-router-dom";

export default function FilterList() {

	const [ filters, setFilters ] = useState([] as FilterType[])

	function load() {
		axios.post('filters/list')
			.then(res => res.data && setFilters(res.data))
	}

	useEffect(() => { load() }, [])

	return (
		<>
		<Header title="Список сохранённых фильтров">
			<Button>add</Button>
		</Header>

		<ViewTable headers={['Имя', 'Примечание']}>
			{filters.map(x => 
				<Link key={x.Id} to={`/filters/details/${x.Id}`}>
					<div>{x.Name}</div>
					<div>{x.Description}</div>
				</Link>
			)}
		</ViewTable>
		</>
	)
}