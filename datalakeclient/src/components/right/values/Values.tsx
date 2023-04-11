import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import valuesApi from "../../../api/valuesApi"
import { useInterval } from "../../../hooks/useInterval"
import { Navigate } from "react-router-dom"
import router from "../../../router/router"
import { ValueRange } from "../../../@types/valueRange"

export default function Values() {

	const [ values, setValues ] = useState([] as ValueRange[])

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
						<th>Тип</th>
						<th>Значение</th>
						<th>Качество</th>
					</tr>
				</thead>
				<tbody>
					{values.map(x => 
					<tr key={x.TagName} onClick={() => router.navigate(`/values/${x.TagName}`)}>
						<td>{x.TagName}</td>
						<td>
							{x.TagType === 0 ? 'Строка' 
							: x.TagType === 1 ? 'Число'
							: x.TagType === 2 ? 'Дискрет'
							: 'Неизвестен'}
						</td>
						{x.Values.length > 0
						? <>
							<td>{x.Values[0].Value}</td>
							<td>{x.Values[0].Quality}</td>
						</> 
						: <>
							<td>?</td>
							<td>0</td>
						</>
						}
					</tr>
					)}
				</tbody>
			</table>
		</div>
	)
}