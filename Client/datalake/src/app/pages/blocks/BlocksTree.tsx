import api from '@/api/swagger-api'
import BlockButton from '@/app/components/buttons/BlockButton'
import { useInterval } from '@/hooks/useInterval'
import { user } from '@/state/user'
import { Button, Input, Table, TableColumnsType } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { AccessType, BlockNestedTagInfo, BlockTreeInfo, BlockWithTagsInfo } from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'

const makeTree = (blocks: BlockWithTagsInfo[]): [BlockTreeInfo[], Record<number, string>] => {
	const meta: Record<number, string> = {}

	const buildHierarchy = (id: number | null, prefix = ''): BlockTreeInfo[] => {
		return blocks
			.filter((block) => block.parentId === id)
			.map((block) => {
				const fullName = prefix ? `${prefix} > ${block.name}` : block.name
				meta[block.id] = fullName
				return {
					...block,
					fullName,
					children: buildHierarchy(block.id, fullName),
				}
			})
			.sort((a, b) => a.name.localeCompare(b.name))
	}

	return [buildHierarchy(null), meta]
}

const EXPAND_KEY = 'expandedBlocks'

const BlocksTree = observer(() => {
	const [data, setData] = useState<BlockWithTagsInfo[]>([])
	const [search, setSearch] = useState('')
	const [expandedRowKeys, setExpandedRowKeys] = useState<number[]>(() => {
		const saved = localStorage.getItem(EXPAND_KEY)
		return saved ? JSON.parse(saved) : []
	})

	// Create tree structure and metadata
	const [tree, meta] = useMemo(() => makeTree(data), [data])

	// Filter and transform data based on search
	const viewData = useMemo(() => {
		if (!search) return tree

		return data
			.filter((block) => block.name?.toLowerCase().includes(search.toLowerCase()))
			.map(
				(block) =>
					({
						...block,
						fullName: meta[block.id] || block.name,
						name: meta[block.id] || block.name,
					}) as BlockTreeInfo,
			)
	}, [search, data, tree, meta])

	// Load blocks data
	const loadBlocks = () => {
		api
			.blocksReadAll()
			.then((res) => setData(res.data))
			.catch(() => setData([]))
	}

	// Initialize and refresh data periodically
	useEffect(loadBlocks, [])
	useInterval(loadBlocks, 60000)

	// Handle expand/collapse of tree nodes
	const handleExpand = (expanded: boolean, record: BlockTreeInfo) => {
		const newKeys = expanded ? [...expandedRowKeys, record.id] : expandedRowKeys.filter((id) => id !== record.id)

		setExpandedRowKeys(newKeys)
	}

	// Persist expanded keys in localStorage
	useEffect(() => {
		localStorage.setItem(EXPAND_KEY, JSON.stringify(expandedRowKeys))
	}, [expandedRowKeys])

	// Create new block
	const createBlock = () => {
		api.blocksCreateEmpty().then(loadBlocks)
	}

	// Table columns configuration
	const columns: TableColumnsType<BlockTreeInfo> = [
		{
			title: (
				<div style={{ display: 'flex', alignItems: 'center' }}>
					<div style={{ padding: '0 1em' }}>Название</div>
					<div style={{ width: '100%' }}>
						<Input
							placeholder='Поиск...'
							value={search}
							onClick={(e) => e.stopPropagation()}
							onChange={(e) => setSearch(e.target.value)}
							className='flex-1'
						/>
					</div>
				</div>
			),
			dataIndex: 'name',
			width: '40%',
			render: (_, record) => <BlockButton block={record} />,
			sorter: (a, b) => a.name.localeCompare(b.name),
			defaultSortOrder: 'ascend',
		},
		{
			title: 'Описание',
			dataIndex: 'description',
		},
		{
			title: 'Теги',
			dataIndex: 'tags',
			render: (tags: BlockNestedTagInfo[]) => tags.length,
			sorter: (a, b) => a.tags.length - b.tags.length,
		},
	]

	return (
		<>
			<PageHeader
				right={
					user.hasGlobalAccess(AccessType.Admin) && (
						<div className='flex gap-2'>
							<NavLink to={routes.blocks.toMoveForm()}>
								<Button>Изменить иерархию</Button>
							</NavLink>
							<Button type='primary' onClick={createBlock}>
								Добавить блок
							</Button>
						</div>
					)
				}
			>
				Блоки верхнего уровня
			</PageHeader>

			<Table
				showSorterTooltip={false}
				size='small'
				columns={columns}
				dataSource={viewData}
				pagination={false}
				expandable={{
					expandedRowKeys,
					onExpand: handleExpand,
				}}
				rowKey='id'
			/>
		</>
	)
})

export default BlocksTree
