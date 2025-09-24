import { TagValueWithInfo } from './TagValueWithInfo'

export type TagViewerModeProps = {
	relations: {
		relationId: string
		value: TagValueWithInfo
	}[]
}
