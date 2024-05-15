import { useEffect, useState } from 'react'
import { Navigate } from 'react-router-dom'
import api from '../../../api/api'
import { TagInfo } from '../../../api/swagger/data-contracts'
import { useUpdateContext } from '../../../context/updateContext'
import { useFetching } from '../../../hooks/useFetching'

export default function TagSelectedForm() {
	const { checkedTags } = useUpdateContext()

	const [tags, setTags] = useState([] as TagInfo[])

	const [load] = useFetching(async () => {
		if (checkedTags.length === 0) return
		api.tagsReadAll({ tags: checkedTags }).then((res) => setTags(res.data))
	})

	useEffect(() => {
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [checkedTags])

	// Если первой страницей была эта, то выбранных тегов быть не может, и мы возвращаемся в корневой элемент
	if (checkedTags.length === 0) {
		return <Navigate to='/' />
	}

	return (
		<>
			{tags.map((x) => (
				<div key={x.id}>{x.name}</div>
			))}
		</>
	)
}
