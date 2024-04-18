import { Button, } from "antd";
import Header from "../../small/Header";
import { Navigate, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { BlockType } from "../../../@types/BlockType";
import { useFetching } from "../../../hooks/useFetching";
import axios, { AxiosResponse } from "axios";
import router from "../../../router/router";
import { useInterval } from "../../../hooks/useInterval";
import FormRow from "../../small/FormRow";
import { TagValue } from "../../../@types/TagValue";
import TagValueElement from "../../small/TagValueEl";

export default function BlockView() {

	const { id } = useParams()
	const [ block, setBlock ] = useState({} as BlockType)
	const [ values, setValues ] = useState([] as { id: number, name: string, type: string, value: any }[])

	const [ load, loading, errLoad ] = useFetching(async () => {
		let res = await axios.post('blocks/read', { id: id }) as AxiosResponse<BlockType, any>
		setBlock(res.data)
		setValues(res.data.Tags.map(x => ({
			id: x.TagId,
			name: x.Name,
			type: x.Type === 0 ? 'входное' : x.Type === 1 ? 'выходное' : 'сопутствующее',
			value: ''
		})))
	})

	const [ getValues ] = useFetching(async () => {
		let res = await axios.post('blocks/live', { id: id })
		let newValues = res.data as TagValue[]
		console.log(values)
		console.log(newValues)
		setValues(values.map(v => ({
			id: v.id,
			name: v.name,
			type: v.type,
			value: newValues.filter(x => x.TagId === v.id)[0].Value
		})))
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { !!id && load() }, [id])
	
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { !!id && (block?.Tags?.length ?? 0) > 0 && getValues() }, [block])
	useInterval(function () { !!id && getValues() }, 1000)

	return (
		errLoad
		? <Navigate to="/offline" />
		: loading
			? <i>загрузка...</i>
			: <>
				<Header
					left={<Button onClick={() => router.navigate('/blocks')}>Вернуться</Button>}
					right={<Button onClick={() => router.navigate('/blocks/edit/' + id)}>Изменить</Button>}
				>
					<i className="material-icons">data_object</i> {block.Name}
				</Header>
				<FormRow title="Описание">
					{block.Description}
				</FormRow>
				<div className="table">
					<div className="table-header">
						<span style={{ width: '40%' }}>Свойство</span>
						<span>Значение</span>
						<span style={{ width: '10em' }}>Тип</span>
					</div>
					{block.Properties && Object.keys(block.Properties).map((key, i) => (
						<div key={i} className="table-row">
							<span>{key}</span>
							<span>{block.Properties[key]}</span>
							<span>постоянное</span>
						</div>
					))}
					{values.map((v, i) => (
						<div key={i} className="table-row">
							<span>{v.name}</span>
							<span><TagValueElement value={v.value} /></span>
							<span>{v.type}</span>
						</div>
					))}
				</div>
			</>
	)
}