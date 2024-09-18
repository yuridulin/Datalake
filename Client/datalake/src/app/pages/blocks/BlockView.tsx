import { Button, Descriptions, DescriptionsProps, Divider, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	BlockInfo,
	BlockTagInfo,
	ValueRecord,
} from '../../../api/swagger/data-contracts'
import { useInterval } from '../../../hooks/useInterval'
import Header from '../../components/Header'
import TagCompactValue from '../../components/TagCompactValue'
import routes from '../../router/routes'

type TableModel = BlockInfo & {
	tags: BlockTagInfo & {
		value?: ValueRecord
	}
}

export default function BlockView() {
	const { id } = useParams()
	const navigate = useNavigate()

	const [block, setBlock] = useState({} as TableModel)

	const items: DescriptionsProps['items'] = [
		{ key: 'name', label: 'Имя', children: block.name },
		{
			key: 'desc',
			label: 'Описание',
			children: block.description,
		},
	]

	const getBlock = () => {
		api.blocksRead(Number(id))
			.then((res) => {
				setBlock(res.data as TableModel)
				getTagsValues(res.data.tags.map((x) => x.id))
			})
			.catch(() => setBlock({} as TableModel))
	}

	const getValues = () => {
		if (block.tags?.length === 0) return
		getTagsValues(block.tags.map((x) => x.id))
	}

	const getTagsValues = (tags: number[]) => {
		api.valuesGet([
			{
				requestKey: 'block-values',
				tagsId: tags,
			},
		]).then((res) => {
			const values = Object.fromEntries(
				res.data[0].tags.map((x) => [x.id, x.values[0]]),
			)
			setBlock({
				...block,
				tags: block.tags.map((x) => ({ ...x, value: values[x.id] })),
			} as unknown as TableModel)
		})
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(getBlock, [id])
	useInterval(getValues, 1000)

	return (
		<>
			<Header
				left={
					<Button onClick={() => navigate('/blocks')}>
						Вернуться
					</Button>
				}
				right={
					<Button onClick={() => navigate('/blocks/edit/' + id)}>
						Редактирование
					</Button>
				}
			>
				{block.name}
			</Header>
			<Descriptions colon={true} layout='vertical' items={items} />
			<Divider />
			<Table dataSource={block.tags} size='small' pagination={false}>
				<Column
					key='guid'
					dataIndex='guid'
					title='Поле'
					render={(_, record: BlockTagInfo) => (
						<NavLink to={routes.Tags.routeToTag(record.guid)}>
							<Button title={record.tagName} size='small'>
								{record.name || <i>имя не задано</i>}
							</Button>
						</NavLink>
					)}
				/>
				<Column
					key='value'
					dataIndex='value'
					title='Значение'
					render={(value: ValueRecord, record: BlockTagInfo) =>
						!value ? (
							<></>
						) : (
							<TagCompactValue
								type={record.tagType!}
								quality={value.quality}
								value={value.value}
							/>
						)
					}
				/>
			</Table>
		</>
	)
}
