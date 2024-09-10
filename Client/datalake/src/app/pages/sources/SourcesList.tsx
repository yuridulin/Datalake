import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import getSourceTypeName from '../../../api/models/getSourceTypeName'
import api from '../../../api/swagger-api'
import { SourceInfo } from '../../../api/swagger/data-contracts'
import Header from '../../components/Header'

export default function SourcesList() {
	const [list, setList] = useState([] as SourceInfo[])

	const load = () => api.sourcesReadAll().then((res) => setList(res.data))

	const createSource = () => {
		api.sourcesCreate().then(load)
	}

	useEffect(() => {
		load()
	}, [])

	return (
		<>
			<Header
				right={
					<Button onClick={createSource}>Добавить источник</Button>
				}
			>
				Зарегистрированные источники данных
			</Header>
			{list.length === 0 ? (
				<div>
					<i>Не определено ни одного источника</i>
				</div>
			) : (
				<div className='table'>
					<div className='table-header'>
						<span>Имя</span>
						<span>Тип</span>
						<span>Адрес</span>
					</div>
					{list.map((x) => (
						<NavLink
							className='table-row'
							to={'/sources/' + x.id}
							key={x.id}
						>
							<span>{x.name}</span>
							<span>{getSourceTypeName(x.type)}</span>
							<span>{x.address}</span>
						</NavLink>
					))}
				</div>
			)}
		</>
	)
}
