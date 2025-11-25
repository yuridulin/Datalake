import TagButton from '@/app/components/buttons/TagButton'
import { Alert } from 'antd'
import { TagSimpleInfo } from '../../generated/data-contracts'

type CreatedTagLinkerProps = {
	tag: TagSimpleInfo
	onClose: (() => void) | undefined
}

export default function CreatedTagLinker({ tag, onClose }: CreatedTagLinkerProps) {
	return (
		<>
			<Alert
				message={
					<>
						<span style={{ marginRight: '1em' }}>Создан новый тег:</span>
						<TagButton tag={tag} />
					</>
				}
				closable
				afterClose={onClose}
			/>
			<br />
		</>
	)
}
