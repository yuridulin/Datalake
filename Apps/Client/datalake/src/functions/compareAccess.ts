import { AccessType } from '@/generated/data-contracts'

export default function compareAccess(a: AccessType, b: AccessType): number {
	const order = (value: AccessType): number => {
		if (value == AccessType.NotSet) return 0
		if (value == AccessType.Viewer) return 1
		if (value == AccessType.Editor) return 2
		if (value == AccessType.Manager) return 3
		if (value == AccessType.Admin) return 4
		return -1
	}

	return order(a) - order(b)
}
