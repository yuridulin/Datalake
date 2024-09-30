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
				to={'/blocks/view/' + record.id}
				key={record.id}
			>
				<Button>{record.name}</Button>
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

export default function BlocksList() {
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

	return (
		<>
			<PageHeader
				right={
					<>
						<NavLink to={routes.Blocks.root + routes.Blocks.Mover}>
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
				rowKey='id'
			/>
		</>
	)
}
