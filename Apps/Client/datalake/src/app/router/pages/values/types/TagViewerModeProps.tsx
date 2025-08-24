import { TagValueWithInfo } from './TagValueWithInfo'

export type TagViewerModeProps = {
	relations: {
		relationId: number
		value: TagValueWithInfo
	}[]
}
