import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	AggregationFunc,
	BlockInfo,
	ValuesRequest,
} from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import { useInterval } from '../../../hooks/useInterval'
import router from '../../../router/router'
import BlockTagRelationEl from '../../small/BlockTagRelationEl'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'
import TagValueElement from '../../small/TagValueEl'

export default function BlockView() {
	const { id } = useParams()
	const [block, setBlock] = useState({} as BlockInfo)
	const [values, setValues] = useState({} as { [key: number]: any })

	const [load, loading, errLoad] = useFetching(async () => {
		api.blocksRead(Number(id)).then((res) => {
			setBlock(res.data)
		})
	})

	const [getValues] = useFetching(async () => {
		api.valuesGet([
			{
				tags: block.tags.map((x) => x.id),
				func: AggregationFunc.List,
			} as ValuesRequest,
		]).then((res) => {
			setValues(
				Object.fromEntries(
					res.data.map((x) => [x.id, x.values[0].value]),
				),
			)
		})
	})

	useEffect(() => {
		!!id && load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	useEffect(() => {
		!!id && (block?.tags?.length ?? 0) > 0 && getValues()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [block])
	useInterval(function () {
		!!id && getValues()
	}, 1000)

	return errLoad ? (
		<Navigate to='/offline' />
	) : loading ? (
		<i>загрузка...</i>
	) : (
		<>
			<Header
				left={
					<Button onClick={() => router.navigate('/blocks')}>
						Вернуться
					</Button>
				}
				right={
					<Button
						onClick={() => router.navigate('/blocks/edit/' + id)}
					>
						Изменить
					</Button>
				}
			>
				<i className='material-icons'>data_object</i> {block.name}
			</Header>
			<FormRow title='Описание'>{block.description}</FormRow>
			<div className='table'>
				<div className='table-header'>
					<span style={{ width: '40%' }}>Свойство</span>
					<span>Значение</span>
					<span style={{ width: '10em' }}>Тип</span>
				</div>
				{block.properties.map((propertyInfo, i) => (
					<div key={i} className='table-row'>
						<span>Свойство</span>
						<span>{propertyInfo.name}</span>
						<span>постоянное</span>
					</div>
				))}
				{block.tags.map((tag, i) => (
					<div key={i} className='table-row'>
						<span>{tag.name}</span>
						<span>
							<TagValueElement value={values[tag.id]} />
						</span>
						<span>
							<BlockTagRelationEl relation={tag.tagType} />
						</span>
					</div>
				))}
			</div>
		</>
	)
}
