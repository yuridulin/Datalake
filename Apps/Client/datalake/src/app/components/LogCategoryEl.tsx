import { Tag } from 'antd'
import { LogCategory } from '../../api/swagger/data-contracts'
import getLogCategoryName from '../../functions/getLogCategoryName'

type LogCategoryProps = {
	category: LogCategory
}

export default function LogCategoryEl({ category }: LogCategoryProps) {
	return <Tag>{getLogCategoryName(category)}</Tag>
}
