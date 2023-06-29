import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { Navigate } from "react-router-dom"
import { Select } from "antd"
import axios from "axios"
import { Tag } from "../../../@types/tag"

export default function Viewer() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ options, setOptions ] = useState([] as { key: number, value: string}[])

	const [ readTags, , error ] = useFetching(async () => {
		let res = await axios.get('tags/list')
		setTags(res.data)
		setOptions(res.data.map((x: Tag) => ({ key: x.Id, value: x.Name })))
	})

	const [ getValues ] = useFetching(async () => {
		//let res = await axios.post('tags/values')
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { readTags() }, [])

	return (
		error
		? <Navigate to="/offline" />
		: tags.length === 0
			? <div><i>не создано ни одного тега</i></div> 
			: <>
			<div>
				<Select 
					mode="tags"
					style={{ width: '100%' }}
					onChange={getValues}
					tokenSeparators={[',', ';', ' ']}
					placeholder="Выберите теги для просмотра..."
					options={options}
				/>
			</div>
		</>
	)
}