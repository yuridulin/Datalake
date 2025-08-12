import { TagValueWithInfo } from '@/app/pages/values/types/TagValueWithInfo'

export type TagViewerModeProps = {
	relations: {
		relationId: number
		value: TagValueWithInfo
	}[]
}
