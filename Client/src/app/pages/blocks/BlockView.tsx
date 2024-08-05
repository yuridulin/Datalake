import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import getDictFromValuesResponseArray from '../../../api/models/getDictFromValuesResponseArray'
import api from '../../../api/swagger-api'
import {
	AggregationFunc,
	BlockInfo,
	ValuesRequest,
} from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import BlockTagRelationEl from '../../components/BlockTagRelationEl'
import FormRow from '../../components/FormRow'
import Header from '../../components/Header'
import TagValueEl from '../../components/TagValueEl'
import router from '../../router/router'

export default function BlockView() {
	const { id } = useParams()
	const [block, setBlock] = useState({} as BlockInfo)
	const [values, setValues] = useState({} as { [key: string]: any })

	function load() {
		api.blocksRead(Number(id)).then((res) => {
			setBlock(res.data)
		})
	}

	function getValues() {
		api.valuesGet([
			{
				tags: block.tags.map((x) => x.guid),
				func: AggregationFunc.List,
			} as ValuesRequest,
		]).then((res) => {
			setValues(getDictFromValuesResponseArray(res.data))
		})
	}

	useEffect(() => {
		if (!id) return
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	useEffect(() => {
		!!id && (block?.tags?.length ?? 0) > 0 && getValues()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [block])
	useInterval(function () {
		!!id && getValues()
	}, 1000)

	return (
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
							<TagValueEl value={values[tag.id]} />
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
