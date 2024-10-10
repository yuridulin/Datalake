import { Button, Descriptions, DescriptionsProps, Divider, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import api from '../../../../api/swagger-api'
import {
	BlockChildInfo,
	BlockFullInfo,
	BlockNestedTagInfo,
	ValueRecord,
} from '../../../../api/swagger/data-contracts'
import { useInterval } from '../../../../hooks/useInterval'
import PageHeader from '../../../components/PageHeader'
import TagCompactValue from '../../../components/TagCompactValue'
import routes from '../../../router/routes'

type BlockValues = {
	[key: number]: ValueRecord
}

export default function BlockView() {
	const { id } = useParams()
	const navigate = useNavigate()

	const [block, setBlock] = useState({} as BlockFullInfo)
	const [values, setValues] = useState({} as BlockValues)

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
				setBlock(res.data)
				getTagsValues(res.data.tags.map((x) => x.id))
			})
			.catch(() => setBlock({} as BlockFullInfo))
	}

	const getValues = () => {
		if (block.tags?.length === 0) return
		getTagsValues(block.tags.map((x) => x.id))
	}

	const getTagsValues = (tags: number[]) => {
		if (tags.length === 0) return
		api.valuesGet([
			{
				requestKey: 'block-values',
				tagsId: tags,
			},
		]).then((res) => {
			const values = Object.fromEntries(
				res.data[0].tags.map((x) => [x.id, x.values[0]]),
			)
			setValues(values)
		})
	}

	const createChild = () => {
		api.blocksCreateEmpty({ parentId: Number(id) }).then(getBlock)
	}

	useEffect(getBlock, [id])
	useInterval(getValues, 5000)

	return (
		<>
			<PageHeader
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
			</PageHeader>
			<Descriptions colon={true} layout='vertical' items={items} />
			<Divider
				variant='dashed'
				orientation='left'
				style={{ fontSize: '1em' }}
			>
				Поля
			</Divider>
			<Table
				dataSource={block.tags}
				size='small'
				pagination={false}
				rowKey='guid'
			>
				<Column
					dataIndex='guid'
					title='Название'
					render={(_, record: BlockNestedTagInfo) => (
						<NavLink to={routes.tags.toTag(record.guid)}>
							<Button title={record.tagName} size='small'>
								{record.name || <i>имя не задано</i>}
							</Button>
						</NavLink>
					)}
				/>
				<Column
					dataIndex='value'
					title='Значение'
					render={(_, record: BlockNestedTagInfo) => {
						const value = values[record.id]
						return !value ? (
							<></>
						) : (
							<TagCompactValue
								type={record.tagType!}
								quality={value.quality}
								value={value.value}
							/>
						)
					}}
				/>
			</Table>
			<Divider
				variant='dashed'
				orientation='left'
				style={{ fontSize: '1em' }}
			>
				Вложенные блоки&emsp;
				<Button size='small' onClick={createChild}>
					Создать
				</Button>
			</Divider>
			{block.children?.length > 0 ? (
				<Table
					dataSource={block.children}
					size='small'
					pagination={false}
					rowKey='id'
				>
					<Column
						dataIndex='id'
						title='Название'
						render={(_, record: BlockChildInfo) => (
							<NavLink
								key={record.id}
								to={routes.blocks.toViewBlock(record.id)}
							>
								<Button size='small'>{record.name}</Button>
							</NavLink>
						)}
					/>
				</Table>
			) : (
				<i>Нет вложенных блоков</i>
			)}
		</>
	)
}
