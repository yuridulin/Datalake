import api from '@/api/swagger-api'
import BlockButton from '@/app/components/buttons/BlockButton'
import { useInterval } from '@/hooks/useInterval'
import { user } from '@/state/user'
import { Button, Input, Table, TableColumnsType } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import {
	AccessType,
	BlockNestedTagInfo,
	BlockSimpleInfo,
	BlockTreeInfo,
	BlockWithTagsInfo,
} from '../../../api/swagger/data-contracts'
import PageHeader from '../../components/PageHeader'
import routes from '../../router/routes'

type DataType = BlockSimpleInfo & {
	tags: BlockNestedTagInfo[]
	children?: DataType[]
}

const makeTree = (blocks: BlockWithTagsInfo[]): [DataType[], Record<number, string>] => {
	const meta = {} as Record<number, string>

	const readBlockChildren = (blocks: BlockWithTagsInfo[], id: number | null, prefix?: string): DataType[] =>
		blocks
			.filter((block) => block.parentId === id)
			.map((block) => {
				const fullName = prefix ? `${prefix} > ${block.name}` : block.name
				meta[block.id] = fullName
				return {
					...block,
					fullName: fullName,
					children: readBlockChildren(blocks, block.id, fullName),
				} as BlockTreeInfo
			})
			.sort((a, b) => a.name.localeCompare(b.name))

	const hierarchy = readBlockChildren(blocks, null)

	return [hierarchy, meta]
}

const BlocksTree = observer(() => {
	const [view, setView] = useState([] as DataType[])
	const [data, setData] = useState([] as BlockWithTagsInfo[])
	const [tree, setTree] = useState([] as DataType[])
	const [meta, setMeta] = useState({} as Record<number, string>)
	const [search, setSearch] = useState('')

	const columns: TableColumnsType<DataType> = [
		{
			title: (
				<div style={{ display: 'flex', alignItems: 'center' }}>
					<div style={{ padding: '0 1em' }}>Название</div>
					<div style={{ width: '100%' }}>
						<Input
							placeholder='Поиск...'
							value={search}
							onClick={(e) => {
								e.preventDefault()
								e.stopPropagation()
							}}
							onChange={(e) => {
								setSearch(e.target.value)
							}}
						/>
					</div>
				</div>
			),
			dataIndex: 'name',
			width: '40%',
			render: (_, record: DataType) => <BlockButton block={record} />,
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

	function load() {
		api
			.blocksReadAll()
			.then((res) => setData(res.data))
			.catch(() => setData([]))
	}

	function createBlock() {
		api.blocksCreateEmpty().then(load)
	}

	useEffect(load, [])
	useInterval(load, 60000)

	useEffect(() => {
		const [treeData, treeMeta] = makeTree(data)
		setTree(treeData)
		setMeta(treeMeta)
	}, [data])

	useEffect(
		() =>
			setView(
				search.length > 0
					? data
							.filter((x) => !!x.name && x.name.toLowerCase().includes(search.toLowerCase()))
							.map((x) => ({ ...x, name: meta[x.id] ?? x.name }))
					: tree,
			),
		[search, data, meta, tree],
	)

	//#region Expand
	const expandKey = 'expandedBlocks'
	const [expandedRowKeys, setExpandedRowKeys] = useState(() => {
		const savedKeys = localStorage.getItem(expandKey)
		return savedKeys ? (JSON.parse(savedKeys) as number[]) : ([] as number[])
	})

	const onExpand = (expanded: boolean, record: DataType) => {
		const keys = expanded ? [...expandedRowKeys, record.id] : expandedRowKeys.filter((key) => key !== record.id)
		setExpandedRowKeys(keys)
	}

	useEffect(() => {
		localStorage.setItem(expandKey, JSON.stringify(expandedRowKeys))
	}, [expandedRowKeys])

	//#endregion

	return (
		<>
			<PageHeader
				right={
					user.hasGlobalAccess(AccessType.Admin) && (
						<>
							<NavLink to={routes.blocks.toMoveForm()}>
								<Button>Изменить иерархию</Button>
							</NavLink>
							&ensp;
							<Button onClick={createBlock}>Добавить блок</Button>
						</>
					)
				}
			>
				Блоки верхнего уровня
			</PageHeader>
			<Table
				showSorterTooltip={false}
				size='small'
				columns={columns}
				dataSource={view}
				pagination={false}
				expandable={{
					expandedRowKeys,
					onExpand,
				}}
				rowKey='id'
			/>
		</>
	)
})

export default BlocksTree
