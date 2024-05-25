import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/api'
import { SourceInfo } from '../../../api/swagger/data-contracts'
import { sourceTypeName } from '../../../api/translators'
import Header from '../../small/Header'

export default function SourcesList() {
	const [list, setList] = useState([] as SourceInfo[])

	const load = () => api.sourcesReadAll().then((res) => setList(res.data))

	const createSource = () => {
		api.sourcesCreate().then(load)
	}

	useEffect(() => {
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
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
							<span>{sourceTypeName(x.type)}</span>
							<span>{x.address}</span>
						</NavLink>
					))}
				</div>
			)}
		</>
	)
}
