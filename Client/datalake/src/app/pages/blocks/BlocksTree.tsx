import { Button, Table, TableColumnsType } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	BlockNestedTagInfo,
	BlockTreeInfo,
} from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'

interface DataType {
	key: React.ReactNode
	id: number
	name: string
	description: string
	tags: BlockNestedTagInfo[]
	children?: DataType[]
}

function transformBlockTreeInfo(blocks: BlockTreeInfo[]): DataType[] {
	const data = blocks.map((block) => {
		const transformedBlock: DataType = {
			key: block.id,
			id: block.id,
			name: block.name,
			description: block.description || '',
			tags: block.tags,
		}

		const children = transformBlockTreeInfo(block.children)
		if (children.length > 0) {
			transformedBlock.children = children
		}

		return transformedBlock
	})

	return data.sort((a, b) => a.name.localeCompare(b.name))
}

const columns: TableColumnsType<DataType> = [
	{
		title: 'Название',
		dataIndex: 'name',
		width: '40%',
		render: (_, record: DataType) => (
			<NavLink
				className='table-row'
				to={routes.blocks.toViewBlock(record.id)}
				key={record.id}
			>
				<Button size='small'>{record.name}</Button>
			</NavLink>
		),
		sorter: (a, b) => a.name.localeCompare(b.name),
		defaultSortOrder: 'ascend',
	},
	{
		title: 'Описание',
		dataIndex: 'description',
	},
	{
		title: 'Количество тегов',
		dataIndex: 'tags',
		render: (_, record: DataType) => record.tags.length,
		sorter: (a, b) => (a.tags.length > b.tags.length ? 1 : -1),
	},
]

export default function BlocksTree() {
	const [data, setData] = useState([] as DataType[])

	function load() {
		api.blocksReadAsTree().then((res) =>
			setData(transformBlockTreeInfo(res.data)),
		)
	}

	function createBlock() {
		api.blocksCreateEmpty().then(() => load())
	}

	useEffect(load, [])

	const expandKey = 'expandedBlocks'
	const [expandedRowKeys, setExpandedRowKeys] = useState(() => {
		const savedKeys = localStorage.getItem(expandKey)
		return savedKeys
			? (JSON.parse(savedKeys) as number[])
			: ([] as number[])
	})

	const onExpand = (expanded: boolean, record: DataType) => {
		const keys = expanded
			? [...expandedRowKeys, record.id]
			: expandedRowKeys.filter((key) => key !== record.id)
		setExpandedRowKeys(keys)
	}

	useEffect(() => {
		localStorage.setItem(expandKey, JSON.stringify(expandedRowKeys))
	}, [expandedRowKeys])

	return (
		<>
			<PageHeader
				right={
					<>
						<NavLink to={routes.blocks.toMoveForm()}>
							<Button>Изменить иерархию</Button>
						</NavLink>
						&ensp;
						<Button onClick={createBlock}>Добавить блок</Button>
					</>
				}
			>
				Блоки верхнего уровня
			</PageHeader>
			<Table
				size='small'
				columns={columns}
				dataSource={data}
				pagination={false}
				expandable={{
					expandedRowKeys,
					onExpand,
				}}
				rowKey='id'
			/>
		</>
	)
}
