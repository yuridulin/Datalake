import { AccessType } from '../swagger/data-contracts'

function hasAccess(current: AccessType, minimal: AccessType): boolean {
	switch (minimal) {
		case AccessType.NotSet:
			return false
		case AccessType.NoAccess:
			return current !== AccessType.NoAccess
		case AccessType.Viewer:
			return (
				current === AccessType.Viewer ||
				current === AccessType.User ||
				current === AccessType.Admin
			)
		case AccessType.User:
			return current === AccessType.User || current === AccessType.Admin
		case AccessType.Admin:
			return current === AccessType.Admin
		default:
			return false
	}
}

export default hasAccess
