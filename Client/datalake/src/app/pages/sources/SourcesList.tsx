import { Button, Table, TableColumnsType } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import getSourceTypeName from '../../../api/models/getSourceTypeName'
import api from '../../../api/swagger-api'
import { SourceInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'

const columns: TableColumnsType<SourceInfo> = [
	{
		dataIndex: 'name',
		title: 'Название',
		render: (_, record) => (
			<NavLink
				className='table-row'
				to={'/sources/' + record.id}
				key={record.id}
			>
				<Button size='small'>{record.name}</Button>
			</NavLink>
		),
	},
	{
		dataIndex: 'type',
		title: 'Тип источника',
		render: (type) => <>{getSourceTypeName(type)}</>,
	},
	{
		dataIndex: 'description',
		title: 'Описание',
	},
]

export default function SourcesList() {
	const [list, setList] = useState([] as SourceInfo[])

	const load = () =>
		api
			.sourcesReadAll({ withCustom: false })
			.then((res) => setList(res.data))

	const createSource = () => {
		api.sourcesCreate().then(load)
	}

	useEffect(() => {
		load()
	}, [])

	return (
		<>
			<PageHeader
				right={
					<Button onClick={createSource}>Добавить источник</Button>
				}
			>
				Зарегистрированные источники данных
			</PageHeader>
			<Table
				dataSource={list}
				columns={columns}
				size='small'
				pagination={false}
				rowKey='id'
			/>
		</>
	)
}
