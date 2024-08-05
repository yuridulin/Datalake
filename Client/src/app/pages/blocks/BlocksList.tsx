import { Button } from 'antd'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { BlockTreeInfo } from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import Header from '../../components/Header'

export default function Dashboard() {
	const [blocks, setBlocks] = useState([] as BlockTreeInfo[])

	const [load] = useFetching(async () => {
		let res = await api.blocksReadAsTree()
		if (res.status === 200) setBlocks(res.data)
	})

	function createBlock() {
		api.blocksCreateEmpty().then(() => load())
	}

	useEffect(() => {
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])

	return (
		<>
			<Header
				right={<Button onClick={createBlock}>Добавить блок</Button>}
			>
				Блоки верхнего уровня
			</Header>
			{blocks.length > 0 ? (
				<div className='table'>
					<div className='table-header'>
						<span>Имя</span>
						<span>Описание</span>
						<span>Кол-во тегов</span>
					</div>
					{blocks.map((x) => (
						<NavLink
							className='table-row'
							to={'/blocks/view/' + x.id}
							key={x.id}
						>
							<span>{x.name}</span>
							<span>{x.description}</span>
							<span>{x.children.length}</span>
						</NavLink>
					))}
				</div>
			) : (
				<div>не определено ни одного блока</div>
			)}
		</>
	)
}
