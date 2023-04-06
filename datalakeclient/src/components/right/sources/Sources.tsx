import { useEffect, useState } from "react"
import sourcesApi from "../../../api/sourcesApi"
import { useFetching } from "../../../hooks/useFetching"
import { Source } from "../../../@types/source"
import { Navigate } from "react-router-dom"
import SourceCreate from "./SourceCreate"
import SourceUpdate from "./SourceUpdate"
import { Button } from "antd"

export default function Sources() {

	const [ list, setList ] = useState([] as Source[])
	const [ id, setId ] = useState(0)

	const [ isCreate, setIsCreate ] = useState(false)
	const [ isUpdate, setIsUpdate ] = useState(false)

	const [ loadList,, error ] = useFetching(async () => {
		let res = await sourcesApi.list()
		if (res) setList(res)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { loadList() }, [])

	return (
		error
		? <Navigate to="/offline" />
		: <div>
			<Button onClick={() => setIsCreate(true)}>Добавить источник</Button>
			<table className="view-table">
				<thead>
					<tr>
						<th>Имя</th>
						<th>Адрес</th>
					</tr>
				</thead>
				<tbody>
					{list.map(x => 
						<tr key={x.Id} onClick={() => { 
							setId(x.Id)
							setIsUpdate(true)
						}}>
							<td>{x.Name}</td>
							<td>{x.Address}</td>
						</tr>
					)}
				</tbody>
			</table>
			<SourceCreate visible={isCreate} setVisible={setIsCreate} loadTable={loadList} />
			<SourceUpdate id={id} visible={isUpdate} setVisible={setIsUpdate} loadTable={loadList} />
		</div>
	)
}