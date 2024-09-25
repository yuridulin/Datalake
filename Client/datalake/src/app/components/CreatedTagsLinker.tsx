import { Alert, Button } from 'antd'
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
							to={routes.Tags.routeToTag(tag.guid)}
							target='_blank'
						>
							<Button>{tag.name}</Button>
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
