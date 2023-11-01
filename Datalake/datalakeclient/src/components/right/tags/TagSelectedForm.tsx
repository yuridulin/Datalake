import { Navigate } from "react-router-dom"
import { useUpdateContext } from "../../../context/updateContext"
import { Tag } from "../../../@types/Tag"
import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import axios from "axios"
import { API } from "../../../router/api"

export default function TagSelectedForm() {

	const { checkedTags } = useUpdateContext()

	const [ tags, setTags ] = useState([] as Tag[])

	const [ load ] = useFetching(async() => {
		if (checkedTags.length === 0) return
		let res = await axios.post(API.tags.getFlatList, { names: checkedTags })
		if (res.data) {
			setTags(res.data)
		}
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [checkedTags])

	// Если первой страницей была эта, то выбранных тегов быть не может, и мы возвращаемся в корневой элемент
	if (checkedTags.length === 0) {
		return <Navigate to="/" />
	}

	return <>
		{tags.map(x => <div key={x.Id}>{x.Name}</div>)}
	</>
}