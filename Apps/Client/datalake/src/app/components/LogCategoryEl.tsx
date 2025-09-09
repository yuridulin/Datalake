import { Tag } from 'antd'
import getLogCategoryName from '../../functions/getLogCategoryName'
import { LogCategory } from '../../generated/data-contracts'

type LogCategoryProps = {
	category: LogCategory
}

export default function LogCategoryEl({ category }: LogCategoryProps) {
	return <Tag>{getLogCategoryName(category)}</Tag>
}
