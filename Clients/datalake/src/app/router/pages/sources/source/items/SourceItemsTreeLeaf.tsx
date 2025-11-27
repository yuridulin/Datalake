import TagButton from '@/app/components/buttons/TagButton'
import { TagType } from '@/generated/data-contracts'
import { MinusCircleOutlined, PlusCircleOutlined } from '@ant-design/icons'
import { Button, Popconfirm } from 'antd'
import { GroupedEntry } from './utils/SourceItems.types'

type SourceItemsTreeLeafProps = {
	group: GroupedEntry
	onCreateTag: (item: string, tagType: TagType) => void
	onDeleteTag: (tagId: number) => void
}

export const SourceItemsTreeLeaf = ({ group, onCreateTag, onDeleteTag }: SourceItemsTreeLeafProps) => {
	return (
		<div style={{ marginLeft: '1.5em' }}>
			{/* все связанные теги */}
			{group.tagInfoArray.map((tag) => (
				<div key={tag.id} style={{ marginTop: '.5em', display: 'flex', alignItems: 'center' }}>
					<TagButton tag={tag} />
					<Popconfirm
						title={
							<>
								Вы уверены, что хотите удалить тег?
								<br />
								Убедитесь, что он не используется где-то еще
							</>
						}
						onConfirm={() => onDeleteTag(tag.id)}
						okText='Да'
						cancelText='Нет'
					>
						<Button size='small' icon={<MinusCircleOutlined />} style={{ marginLeft: 8 }} />
					</Popconfirm>
				</div>
			))}
			{/* кнопка создания нового тега */}
			{group.itemInfo && (
				<div style={{ marginTop: '.5em', display: 'flex', alignItems: 'center' }}>
					<Button
						size='small'
						icon={<PlusCircleOutlined />}
						onClick={() => onCreateTag(group.path, group.itemInfo!.type || TagType.String)}
					>
						Создать тег
					</Button>
				</div>
			)}
		</div>
	)
}
