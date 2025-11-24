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
import { useAppStore } from '@/store/useAppStore'
import { RightOutlined } from '@ant-design/icons'
import { Button, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useRef, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'

const childrenContainerStyle = {
	marginBottom: '1em',
}

const BlockView = observer(() => {
	const store = useAppStore()
	const { id } = useParams()
	useDatalakeTitle('Блоки', '#' + id)

	const [ready, setReady] = useState(false)
	const [block, setBlock] = useState({} as BlockDetailedInfo)
	const hasLoadedRef = useRef(false)
	const lastIdRef = useRef<string | undefined>(id)

	const items: InfoTableProps['items'] = {
		Имя: block.name,
		Описание: block.description || <i>нет</i>,
	}

	const getBlock = () => {
		setReady(false)
		store.api
			.inventoryBlocksGet(Number(id))
			.then((res) => {
				res.data.adults = res.data.adults.reverse()
				setBlock(res.data)
				setReady(true)
			})
			.catch(() => setBlock({} as BlockDetailedInfo))
	}

	const createChild = () => {
		store.api.inventoryBlocksCreate({ parentId: Number(id) }).then(getBlock)
	}

	useEffect(() => {
		// Если изменился id, сбрасываем флаг загрузки
		if (lastIdRef.current !== id) {
			hasLoadedRef.current = false
			lastIdRef.current = id
		}

		if (hasLoadedRef.current || !id) return
		hasLoadedRef.current = true
		getBlock()
	}, [store.api, id])

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

	return !ready ? (
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
							block.adults.length > 0 ? (
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
