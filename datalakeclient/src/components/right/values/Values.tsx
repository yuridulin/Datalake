import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { Value } from "../../../@types/value"
import valuesApi from "../../../api/valuesApi"
import { useInterval } from "../../../hooks/useInterval"
import { Navigate } from "react-router-dom"
import router from "../../../router/router"

export default function Values() {

	const [ values, setValues ] = useState([] as Value[])

	const [ load, , error ] = useFetching(async () => {
		let data = await valuesApi.list({ tags: [] })
		if (data) setValues(data)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])
	useInterval(() => { load() }, 1000)

	return (
		error
		?
		<Navigate to="/offline" />
		:
		<div>
			<table className="view-table">
				<thead>
					<tr>
						<th>Тег</th>
						<th>Строковое значение</th>
						<th>Числовое значение</th>
						<th>Качество</th>
					</tr>
				</thead>
				<tbody>
					{values.map(x => 
					<tr onClick={() => router.navigate(`/values/${x.TagName}`)}>
						<td>{x.TagName}</td>
						<td>{x.Text}</td>
						<td>{x.Number}</td>
						<td>{x.Quality}</td>
					</tr>
					)}
				</tbody>
			</table>
		</div>
	)
}