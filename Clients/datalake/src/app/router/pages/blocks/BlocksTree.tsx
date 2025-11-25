import BlockButton from '@/app/components/buttons/BlockButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import PageHeader from '@/app/components/PageHeader'
import ProtectedButton from '@/app/components/ProtectedButton'
import { AccessType, BlockNestedTagInfo, BlockTreeInfo, BlockWithTagsInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Input, Table, TableColumnsType } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useLocalStorage } from 'react-use'
import routes from '../../routes'

const makeTree = (blocks: BlockWithTagsInfo[]): [BlockTreeInfo[] | null, Record<number, string>] => {
	const meta: Record<number, string> = {}

	const buildHierarchy = (id: number | null, prefix = ''): BlockTreeInfo[] => {
		const hierarchy = blocks
			.filter((block) => block.parentBlockId === id)
			.map((block) => {
				const fullName = prefix ? `${prefix} > ${block.name}` : block.name
				meta[block.id] = fullName
				const children = buildHierarchy(block.id, fullName)
				return {
					...block,
					children: children.length > 0 ? children : [],
				}
			})
			.sort((a, b) => a.name.localeCompare(b.name))

		return hierarchy
	}

	const treeResult = buildHierarchy(null)
	return [treeResult.length > 0 ? treeResult : null, meta]
}

const EXPAND_KEY = 'expandedBlocks'

const BlocksTree = observer(() => {
	useDatalakeTitle('Блоки')

	const store = useAppStore()
	const [search, setSearch] = useState('')
	const [expandedRowKeys, setExpandedRowKeys] = useLocalStorage(EXPAND_KEY, [] as number[])

	// Получаем данные из store (реактивно через MobX)
	const data = store.blocksStore.getBlocks()

	// Create tree structure and metadata
	const [tree, meta] = useMemo(() => makeTree(data), [data])

	// Filter and transform data based on search
	const viewData = useMemo((): BlockTreeInfo[] => {
		if (!search) return tree ?? []

		return data
			.filter((block) => block.name?.toLowerCase().includes(search.toLowerCase()))
			.map(
				(block) =>
					({
						...block,
						name: meta[block.id] || block.name,
						//children: [],
					}) as BlockTreeInfo,
			)
	}, [search, data, tree, meta])

	// Handle expand/collapse of tree nodes
	const handleExpand = (expanded: boolean, record: BlockTreeInfo) => {
		const exists = expandedRowKeys ?? []
		const newKeys = expanded ? [...exists, record.id] : exists.filter((id) => id !== record.id)

		setExpandedRowKeys(newKeys)
	}

	// Create new block
	const createBlock = async () => {
		try {
			await store.api.inventoryBlocksCreate({})
			// Инвалидируем кэш и обновляем данные
			await store.blocksStore.refreshBlocks()
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create block'), {
				component: 'BlocksTree',
				action: 'handleCreateBlock',
			})
		}
	}

	// Обновляем данные при переходе на страницу
	useEffect(() => {
		store.blocksStore.refreshBlocks().catch((error) => {
			logger.error(error instanceof Error ? error : new Error(String(error)), {
				component: 'BlocksTree',
				action: 'refreshBlocks',
			})
		})
	}, [store.blocksStore])

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
				right={[
					<ProtectedButton access={store.getGlobalAccess()} required={AccessType.Admin}>
						<NavLink to={routes.blocks.toMoveForm()}>Изменить иерархию</NavLink>
					</ProtectedButton>,
					<ProtectedButton
						access={store.getGlobalAccess()}
						required={AccessType.Admin}
						type='primary'
						onClick={createBlock}
					>
						Добавить блок
					</ProtectedButton>,
				]}
			>
				Блоки верхнего уровня
			</PageHeader>

			<PollingLoader pollingFunction={() => store.blocksStore.refreshBlocks()} interval={60000} />
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
