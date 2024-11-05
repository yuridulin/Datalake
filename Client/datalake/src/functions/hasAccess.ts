import { AccessType } from '../api/swagger/data-contracts'

function hasAccess(current: AccessType, minimal: AccessType): boolean {
	switch (minimal) {
		case AccessType.NotSet:
			return true
		case AccessType.NoAccess:
			return current !== AccessType.NoAccess
		case AccessType.Viewer:
			return (
				current === AccessType.Viewer ||
				current === AccessType.Editor ||
				current === AccessType.Manager ||
				current === AccessType.Admin
			)
		case AccessType.Editor:
			return (
				current === AccessType.Editor ||
				current === AccessType.Manager ||
				current === AccessType.Admin
			)
		case AccessType.Manager:
			return (
				current === AccessType.Manager || current === AccessType.Admin
			)
		case AccessType.Admin:
			return current === AccessType.Admin
		default:
			return false
	}
}

export default hasAccess
