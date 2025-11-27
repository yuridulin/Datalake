import BlockButton from '@/app/components/buttons/BlockButton'
import BlockIcon from '@/app/components/icons/BlockIcon'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import { encodeBlockTagPair, FlattenedNestedTagsType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagsValuesViewer from '@/app/components/values/TagsValuesViewer'
import routes from '@/app/router/routes'
import { AccessType, BlockDetailedInfo, BlockSimpleInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { RightOutlined } from '@ant-design/icons'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo } from 'react'
import { NavLink, useParams } from 'react-router-dom'

const childrenContainerStyle = {
	marginBottom: '1em',
}

const BlockView = observer(() => {
	const { id } = useParams()
	const blockId = Number(id)
	useDatalakeTitle('Блоки', '#' + blockId)

	const store = useAppStore()

	useEffect(() => {
		store.blocksStore.refreshDetailedById(blockId)
	}, [blockId, store.blocksStore])

	const blockData = store.blocksStore.getDetailedById(blockId)

	const block = useMemo(() => {
		if (!blockData) return {} as BlockDetailedInfo
		// Создаем копию, чтобы не мутировать оригинал
		const processed = { ...blockData }
		if (processed.adults) {
			processed.adults = [...processed.adults].reverse()
		}
		return processed
	}, [blockData])

	const items: InfoTableProps['items'] = {
		Имя: block.name,
		Описание: block.description || <i>нет</i>,
	}

	const createChild = async () => {
		try {
			store.blocksStore.createBlock({ parentId: blockId })
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to create child block'), {
				component: 'BlockView',
				action: 'handleCreateChild',
				parentBlockId: blockId,
			})
		}
	}

	// Создаем маппинг тегов для TagValuesViewer
	const tagMapping = useMemo(() => {
		const mapping: FlattenedNestedTagsType = {}
		block.tags?.forEach((tag) => {
			const tagId = tag.tag?.id ?? tag.tagId ?? 0
			const value = encodeBlockTagPair(block.id, tagId)
			const tagInfo = tag.tag
			mapping[value] = {
				...tag,
				blockId: block.id,
				localName: tag.localName ?? tagInfo?.name ?? '',
			}
		})
		return mapping
	}, [block.tags, block.id])

	const relations = useMemo(
		() =>
			block.tags?.map((tag) => {
				const tagId = tag.tag?.id ?? tag.tagId ?? 0
				return encodeBlockTagPair(block.id, tagId)
			}) || [],
		[block.tags, block.id],
	)

	return !blockData ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={[
					<NavLink to={routes.blocks.list}>
						<Button>К дереву блоков</Button>
					</NavLink>,
				]}
				right={[
					store.hasAccessToBlock(AccessType.Editor, Number(id)) && (
						<NavLink to={routes.blocks.toEditBlock(Number(id))}>
							<Button>Редактирование блока</Button>
						</NavLink>
					),
					store.hasAccessToBlock(AccessType.Admin, Number(id)) && (
						<NavLink to={routes.blocks.toBlockAccessForm(Number(id))}>
							<Button>Редактирование разрешений</Button>
						</NavLink>
					),
				]}
				icon={<BlockIcon />}
			>
				{block.name}
			</PageHeader>

			<InfoTable items={items} />
			<br />

			<TabsView
				items={[
					{
						key: 'history',
						label: 'История значений',
						children: <TagsValuesViewer relations={relations} tagMapping={tagMapping} integrated={true} />,
					},
					{
						key: 'parents',
						label: 'Родительские блоки',
						children:
							block.adults && block.adults.length > 0 ? (
								<div style={{ display: 'flex' }}>
									<BlockButton block={block.adults[0]} />
									{block.adults.slice(1).map((x: BlockSimpleInfo) => (
										<div key={x.id}>
											<RightOutlined style={{ margin: '0 1em', fontSize: '7px' }} />
											<BlockButton block={x} />
										</div>
									))}
									<RightOutlined style={{ margin: '0 1em', fontSize: '7px' }} />
									<Button size='small' disabled>
										{block.name}
									</Button>
								</div>
							) : (
								<div style={childrenContainerStyle}>
									<i>Это блок верхнего уровня</i>
								</div>
							),
					},
					{
						key: 'nested',
						label: 'Дочерние блоки',
						children: (
							<>
								{block.children?.length > 0 ? (
									block.children.map((record: BlockSimpleInfo, i: number) => (
										<div key={i} style={childrenContainerStyle}>
											<BlockButton
												key={record.id}
												block={{
													id: record.id ?? 0,
													name: record.name ?? '',
													guid: record.guid ?? '',
													accessRule: record.accessRule,
												}}
											/>
										</div>
									))
								) : (
									<div style={childrenContainerStyle}>
										<i>Нет дочерних блоков</i>
									</div>
								)}
								{store.hasAccessToBlock(AccessType.Manager, Number(id)) && (
									<div style={childrenContainerStyle}>
										<Button size='small' onClick={createChild}>
											Создать
										</Button>
									</div>
								)}
							</>
						),
					},
					{
						key: 'logs',
						label: 'События',
						children: <LogsTableEl blockId={block.id} />,
					},
				]}
			/>
		</>
	)
})

export default BlockView
