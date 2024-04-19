import { Button } from 'antd'
import axios from 'axios'
import { useEffect, useState } from 'react'
import { NavLink, Navigate } from 'react-router-dom'
import { BlockTreeInfo } from '../../../api/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import Header from '../../small/Header'

export default function Dashboard() {
	const [blocks, setBlocks] = useState([] as BlockTreeInfo[])

	const [load, , error] = useFetching(async () => {
		let res = await axios.post('blocks/list')
		setBlocks(res.data)
	})

	const [createBlock] = useFetching(async () => {
		let res = await axios.post('blocks/create', { ParentId: 0 })
		if (res.data.Done) load()
	})

	useEffect(() => {
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])

	return error ? (
		<Navigate to='' />
	) : (
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
