import api from '@/api/swagger-api'
import {
	AccessType,
	BlockChildInfo,
	BlockFullInfo,
	BlockNestedTagInfo,
	ValueRecord,
} from '@/api/swagger/data-contracts'
import BlockButton from '@/app/components/buttons/BlockButton'
import PageHeader from '@/app/components/PageHeader'
import TagCompactValue from '@/app/components/TagCompactValue'
import routes from '@/app/router/routes'
import { useInterval } from '@/hooks/useInterval'
import { user } from '@/state/user'
import { RightOutlined } from '@ant-design/icons'
import {
	Button,
	Descriptions,
	DescriptionsProps,
	Divider,
	Spin,
	Table,
} from 'antd'
import Column from 'antd/es/table/Column'
import { observer } from 'mobx-react-lite'
import { useEffect, useState } from 'react'
import { NavLink, useParams } from 'react-router-dom'

type BlockValues = {
	[key: number]: ValueRecord
}

const dividerStyle = {
	fontSize: '1em',
	marginTop: '2em',
}

const BlockView = observer(() => {
	const { id } = useParams()

	const [ready, setReady] = useState(false)
	const [block, setBlock] = useState({} as BlockFullInfo)
	const [values, setValues] = useState({} as BlockValues)

	const items: DescriptionsProps['items'] = [
		{ key: 'name', label: 'Имя', children: block.name },
		{
			key: 'desc',
			label: 'Описание',
			children: block.description,
		},
	]

	const getBlock = () => {
		setReady(false)
		api.blocksRead(Number(id))
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
		api.valuesGet([
			{
				requestKey: 'block-values',
				tagsId: tags,
			},
		]).then((res) => {
			const values = Object.fromEntries(
				res.data[0].tags.map((x) => [x.id, x.values[0]]),
			)
			setValues(values)
		})
	}

	const createChild = () => {
		api.blocksCreateEmpty({ parentId: Number(id) }).then(getBlock)
	}

	useEffect(getBlock, [id])
	useInterval(getValues, 5000)

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
						{user.hasAccessToBlock(
							AccessType.Editor,
							Number(id),
						) && (
							<NavLink to={routes.blocks.toEditBlock(Number(id))}>
								<Button>Редактирование блока</Button>
							</NavLink>
						)}
						&ensp;
						{user.hasAccessToBlock(
							AccessType.Admin,
							Number(id),
						) && (
							<NavLink
								to={routes.blocks.toBlockAccessForm(Number(id))}
							>
								<Button>Редактирование разрешений</Button>
							</NavLink>
						)}
					</>
				}
			>
				{block.name}
			</PageHeader>

			<Descriptions colon={true} layout='vertical' items={items} />

			<Divider variant='dashed' orientation='left' style={dividerStyle}>
				Вышестоящие блоки
			</Divider>
			{block.adults.length > 0 ? (
				<div style={{ display: 'flex' }}>
					<BlockButton block={block.adults[0]} />
					{block.adults.slice(1).map((x) => (
						<>
							<RightOutlined
								style={{ margin: '0 1em', fontSize: '7px' }}
							/>
							<BlockButton block={x} />
						</>
					))}
					<RightOutlined
						style={{ margin: '0 1em', fontSize: '7px' }}
					/>
					<Button size='small' disabled>
						{block.name}
					</Button>
				</div>
			) : (
				<i>Это блок верхнего уровня</i>
			)}

			<Divider variant='dashed' orientation='left' style={dividerStyle}>
				Вложенные блоки
				{user.hasAccessToBlock(AccessType.Manager, Number(id)) && (
					<>
						&emsp;
						<Button size='small' onClick={createChild}>
							Создать
						</Button>
					</>
				)}
			</Divider>
			{block.children?.length > 0 ? (
				<Table
					dataSource={block.children}
					size='small'
					pagination={false}
					rowKey='id'
				>
					<Column
						dataIndex='id'
						title='Название'
						render={(_, record: BlockChildInfo) => (
							<NavLink
								key={record.id}
								to={routes.blocks.toViewBlock(record.id)}
							>
								<Button size='small'>{record.name}</Button>
							</NavLink>
						)}
					/>
				</Table>
			) : (
				<i>Нет вложенных блоков</i>
			)}

			<Divider variant='dashed' orientation='left' style={dividerStyle}>
				Поля
			</Divider>
			<Table
				dataSource={block.tags}
				size='small'
				pagination={false}
				rowKey='guid'
			>
				<Column
					dataIndex='guid'
					title='Название'
					render={(_, record: BlockNestedTagInfo) => (
						<NavLink to={routes.tags.toTagForm(record.guid)}>
							<Button title={record.tagName} size='small'>
								{record.name || <i>имя не задано</i>}
							</Button>
						</NavLink>
					)}
				/>
				<Column
					dataIndex='value'
					title='Значение'
					render={(_, record: BlockNestedTagInfo) => {
						const value = values[record.id]
						return !value ? (
							<></>
						) : (
							<TagCompactValue
								type={record.tagType!}
								quality={value.quality}
								value={value.value}
							/>
						)
					}}
				/>
			</Table>
		</>
	)
})

export default BlockView
