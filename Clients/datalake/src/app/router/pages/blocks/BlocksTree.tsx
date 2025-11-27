import BlockButton from '@/app/components/buttons/BlockButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import PageHeader from '@/app/components/PageHeader'
import ProtectedButton from '@/app/components/ProtectedButton'
import { AccessType, BlockNestedTagInfo, BlockTreeInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Input, Table, TableColumnsType } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { NavLink } from 'react-router-dom'
import { useLocalStorage } from 'react-use'
import routes from '../../routes'

const EXPAND_KEY = 'expandedBlocks'

const BlocksTree = observer(() => {
	useDatalakeTitle('Блоки')

	const store = useAppStore()
	const [search, setSearch] = useState('')

	const [expandedRowKeys, setExpandedRowKeys] = useLocalStorage(EXPAND_KEY, [] as number[])
	const tree = store.blocksStore.searchBlocks(search)

	const handleExpand = (expanded: boolean, record: BlockTreeInfo) => {
		const exists = expandedRowKeys ?? []
		const newKeys = expanded ? [...exists, record.id] : exists.filter((id) => id !== record.id)

		setExpandedRowKeys(newKeys)
	}

	const refreshFunc = useCallback(() => store.blocksStore.refreshBlocks(), [store.blocksStore])
	useEffect(refreshFunc, [refreshFunc])

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
						onClick={store.blocksStore.createBlock}
					>
						Добавить блок верхнего уровня
					</ProtectedButton>,
				]}
			>
				Блоки верхнего уровня
			</PageHeader>

			<PollingLoader pollingFunction={refreshFunc} interval={5000} />
			<Table
				showSorterTooltip={false}
				size='small'
				columns={columns}
				dataSource={tree}
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
