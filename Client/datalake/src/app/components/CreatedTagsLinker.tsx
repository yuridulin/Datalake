import TagButton from '@/app/components/buttons/TagButton'
import { Alert } from 'antd'
import { NavLink } from 'react-router-dom'
import { TagInfo } from '../../api/swagger/data-contracts'
import routes from '../router/routes'

type CreatedTagLinkerProps = {
	tag: TagInfo
	onClose: (() => void) | undefined
}

export default function CreatedTagLinker({
	tag,
	onClose,
}: CreatedTagLinkerProps) {
	return (
		<>
			<Alert
				message={
					<>
						Создан тег:
						<NavLink
							to={routes.tags.toTagForm(tag.guid)}
							target='_blank'
						>
							<TagButton tag={tag} />
						</NavLink>
						. Нажмите, чтобы перейти к нему.
					</>
				}
				closable
				afterClose={onClose}
			></Alert>
			<br />
		</>
	)
}
