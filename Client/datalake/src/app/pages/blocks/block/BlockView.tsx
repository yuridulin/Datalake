import api from '@/api/swagger-api'
import { AccessType, BlockFullInfo, BlockNestedTagInfo, TagType, ValueRecord } from '@/api/swagger/data-contracts'
import BlockButton from '@/app/components/buttons/BlockButton'
import TagButton from '@/app/components/buttons/TagButton'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import TagCompactValue from '@/app/components/TagCompactValue'
import { TagValuesViewer } from '@/app/components/values/TagValuesViewer'
import routes from '@/app/router/routes'
import { useInterval } from '@/hooks/useInterval'
import { user } from '@/state/user'
import { RightOutlined } from '@ant-design/icons'
import { Button, Spin, Table } from 'antd'
import Column from 'antd/es/table/Column'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'

type BlockValues = {
	[key: number]: ValueRecord
}

const childrenContainerStyle = {
	marginBottom: '1em',
}

const BlockView = observer(() => {
	const { id } = useParams()

	const [ready, setReady] = useState(false)
	const [block, setBlock] = useState({} as BlockFullInfo)
	const [values, setValues] = useState({} as BlockValues)

	const items: InfoTableProps['items'] = {
		Имя: block.name,
		Описание: block.description ?? <i>нет</i>,
	}

	const getBlock = () => {
		setReady(false)
		api
			.blocksRead(Number(id))
			.then((res) => {
				res.data.adults = res.data.adults.reverse()
				setBlock(res.data)
				getTagsValues(res.data.tags.map((x) => x.id))
				setReady(true)
			})
			.catch(() => setBlock({} as BlockFullInfo))
	}

	const getValues = () => {
		if (block.tags?.length === 0) return
		getTagsValues(block.tags.map((x) => x.id))
	}

	const getTagsValues = (tags: number[]) => {
		if (tags.length === 0) return
		api
			.valuesGet([
				{
					requestKey: 'block-values',
					tagsId: tags,
				},
			])
			.then((res) => {
				const values = Object.fromEntries(res.data[0].tags.map((x) => [x.id, x.values[0]]))
				setValues(values)
			})
	}

	const createChild = () => {
		api.blocksCreateEmpty({ parentId: Number(id) }).then(getBlock)
	}

	useEffect(getBlock, [id])
	useInterval(getValues, 5000)

	// Создаем маппинг тегов для TagValuesViewer
	const tagMapping = useMemo(() => {
		const mapping: Record<number, { id: number; localName: string; type: TagType }> = {}
		block.tags?.forEach((tag) => {
			mapping[tag.relationId] = {
				id: tag.id,
				localName: tag.localName,
				type: tag.type,
			}
		})
		return mapping
	}, [block.tags])

	const relations = useMemo(() => block.tags?.map((tag) => tag.relationId) || [], [block.tags])

	return !ready ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<NavLink to={routes.blocks.list}>
						<Button>К дереву блоков</Button>
					</NavLink>
				}
				right={
					<>
						{user.hasAccessToBlock(AccessType.Editor, Number(id)) && (
							<NavLink to={routes.blocks.toEditBlock(Number(id))}>
								<Button>Редактирование блока</Button>
							</NavLink>
						)}
						&ensp;
						{user.hasAccessToBlock(AccessType.Admin, Number(id)) && (
							<NavLink to={routes.blocks.toBlockAccessForm(Number(id))}>
								<Button>Редактирование разрешений</Button>
							</NavLink>
						)}
					</>
				}
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
						children: <TagValuesViewer relations={relations} tagMapping={tagMapping} integrated={true} />,
					},
					{
						key: 'fields',
						label: 'Поля',
						children: (
							<Table dataSource={block.tags} size='small' pagination={false} rowKey='relationId'>
								<Column
									dataIndex='id'
									title='Название'
									render={(_, record: BlockNestedTagInfo) => <TagButton tag={{ ...record, name: record.localName }} />}
								/>
								<Column
									dataIndex='value'
									title='Значение'
									render={(_, record: BlockNestedTagInfo) => {
										const value = values[record.id]
										return !value ? (
											<></>
										) : (
											<TagCompactValue type={record.type} quality={value.quality} value={value.value} />
										)
									}}
								/>
							</Table>
						),
					},
					{
						key: 'parents',
						label: 'Родительские блоки',
						children:
							block.adults.length > 0 ? (
								<div style={{ display: 'flex' }}>
									<BlockButton block={block.adults[0]} />
									{block.adults.slice(1).map((x) => (
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
									block.children.map((record, i) => (
										<div key={i} style={childrenContainerStyle}>
											<BlockButton
												key={record.id}
												block={{
													id: record.id ?? 0,
													name: record.name ?? '',
													guid: '',
												}}
											/>
										</div>
									))
								) : (
									<div style={childrenContainerStyle}>
										<i>Нет дочерних блоков</i>
									</div>
								)}
								{user.hasAccessToBlock(AccessType.Manager, Number(id)) && (
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
