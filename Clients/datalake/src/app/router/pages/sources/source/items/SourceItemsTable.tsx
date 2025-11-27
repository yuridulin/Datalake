import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import compareValues, { compareRecords } from '@/functions/compareValues'
import { TagType } from '@/generated/data-contracts'
import { CheckCircleOutlined, CloseCircleOutlined, MinusCircleOutlined, PlusCircleOutlined } from '@ant-design/icons'
import { Button, Popconfirm, Table, TableColumnsType, Tag, theme } from 'antd'
import dayjs from 'dayjs'
import { SourceEntryInfo } from './utils/SourceItems.types'

type SourceItemsTableProps = {
	items: SourceEntryInfo[]
	paginationConfig: { pageSize: number; current: number }
	onPaginationChange: (page: number, pageSize: number) => void
	onCreateTag: (item: string, tagType: TagType) => void
	onDeleteTag: (tagId: number) => void
}

export const SourceItemsTable = ({
	items,
	paginationConfig,
	onPaginationChange,
	onCreateTag,
	onDeleteTag,
}: SourceItemsTableProps) => {
	const { token } = theme.useToken()

	const columns: TableColumnsType<SourceEntryInfo> = [
		{
			dataIndex: ['itemInfo', 'item'],
			title: 'Путь в источнике',
			width: '30%',
			render: (_, record) => <>{record.itemInfo?.path ?? <Tag>Путь не существует</Tag>}</>,
			sorter: (a, b) => compareValues(a.itemInfo?.path, b.itemInfo?.path),
		},
		{
			dataIndex: ['itemInfo', 'value'],
			title: 'Последнее значение',
			width: '20em',
			render: (_, record) =>
				record.itemInfo ? (
					<TagCompactValue
						type={record.itemInfo.type}
						quality={record.itemInfo.value.quality}
						record={record.itemInfo.value}
					/>
				) : (
					<></>
				),
			sorter: (a, b) => compareRecords(a.itemInfo?.value, b.itemInfo?.value),
		},
		{
			dataIndex: ['tagInfo', 'guid'],
			title: 'Сопоставленные теги',
			render: (_, record) =>
				!record.tagInfo ? (
					<span>
						<Button
							size='small'
							icon={<PlusCircleOutlined />}
							onClick={() => onCreateTag(record.itemInfo?.path ?? '', record.itemInfo?.type || TagType.String)}
						></Button>
					</span>
				) : (
					<span>
						<TagButton tag={record.tagInfo} />
						<Popconfirm
							title={
								<>
									Вы уверены, что хотите удалить тег?
									<br />
									Убедитесь, что он не используется где-то еще, перед удалением
								</>
							}
							placement='bottom'
							onConfirm={() => onDeleteTag(record.tagInfo!.id)}
							okText='Да'
							cancelText='Нет'
						>
							<Button size='small' icon={<MinusCircleOutlined />}></Button>
						</Popconfirm>
					</span>
				),
			sorter: (a, b) => compareValues(a.tagInfo?.name, b.tagInfo?.name),
		},
		{
			dataIndex: ['isTagInUse'],
			width: '3em',
			align: 'center',
			title: (
				<span title='Показывает, используется ли тег в запросах получения данных, внутренних или внешних. Подробнее о запросах можно узнать, перейдя на страницу просмотра информации о теге'>
					Исп.
				</span>
			),
			render: (_, record) =>
				record.tagInfo ? (
					record.isTagInUse && dayjs(record.isTagInUse).add(30, 'minute') > dayjs() ? (
						<CheckCircleOutlined style={{ color: token.colorSuccess }} title='Тег используется' />
					) : (
						<CloseCircleOutlined title='Тег не используется' />
					)
				) : (
					<></>
				),
			sorter: (a, b) => compareValues(a.tagInfo && a.isTagInUse, b.tagInfo && b.isTagInUse),
		},
	]

	return (
		<Table
			dataSource={items}
			columns={columns}
			showSorterTooltip={false}
			size='small'
			pagination={{
				...paginationConfig,
				position: ['bottomCenter'],
				showSizeChanger: true,
				onChange: onPaginationChange,
			}}
			rowKey={(row) => (row.itemInfo?.path ?? '') + (row.tagInfo?.guid ?? String(row.tagInfo?.id ?? ''))}
		/>
	)
}
