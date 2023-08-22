import { Button } from "antd"
import { useState, useEffect } from "react"
import { useFetching } from "../../../hooks/useFetching"
import axios from "axios"
import { Tag } from "../../../@types/Tag"
import { PlusCircleOutlined } from "@ant-design/icons"
import { NavLink } from "react-router-dom"
import { SourceType } from "../../../@types/enums/SourceType"

export default function SourceItems({ type, id }: { type: keyof typeof SourceType, id: number }) {

	const [ items, setItems ] = useState([] as { Item: string, Tag?: Tag }[])

	const [ read, , error ] = useFetching(async () => {
		let res = await axios.post('sources/tags/', { id })
		setItems(res.data)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { !!id && read() }, [id])

	const createTag = async (item: string) => {
		let res = await axios.post('tags/createFromSource', { SourceId: id, SourceItem: item })
		if (res.data.Done) read()
	}

	return (
		error
		? <div><i>Источник данных не предоставил информацию о доступных значениях</i></div>
		: <>
			<div className="table">
				<div className="table-caption">Доступные значения с этого источника данных</div>
				{items.map(x => <div className="table-row">
					<span>{x.Item}</span>
					{!!x.Tag
					? <span>
						<NavLink to={'/tags/' + x.Tag.Id}>
							<Button>{x.Tag.Name}</Button>
						</NavLink>
					</span>
					: <span>
						<Button icon={<PlusCircleOutlined />} onClick={() => createTag(x.Item)}></Button>
					</span>}
				</div>)}
			</div>
		</>
	)
}