import { Button, Table, TableColumnsType } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { BlockTreeInfo } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'

interface DataType {
	key: React.ReactNode
	id: number
	name: string
	description: string
	children?: DataType[]
}

function transformBlockTreeInfo(blocks: BlockTreeInfo[]): DataType[] {
	const data = blocks.map((block) => {
		const transformedBlock: DataType = {
			key: block.id,
			id: block.id,
			name: block.name,
			description: block.description || '',
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
		key: 'id',
		width: '3em',
		align: 'center',
	},
	{
		title: 'Название',
		dataIndex: 'name',
		key: 'name',
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
		key: 'desc',
	},
]

export default function Dashboard() {
	const [data, setData] = useState([] as DataType[])

	function load() {
		api.blocksReadAsTree().then((res) =>
			setData(transformBlockTreeInfo(res.data)),
		)
	}

	function createBlock() {
		api.blocksCreateEmpty().then(() => load())
	}

	useEffect(() => {
		load()
	}, [])

	return (
		<>
			<Header
				right={<Button onClick={createBlock}>Добавить блок</Button>}
			>
				Блоки верхнего уровня
			</Header>
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
