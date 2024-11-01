import { Tag } from 'antd'
import getLogCategodyName from '../../api/functions/getLogCategoryName'
import { LogCategory } from '../../api/swagger/data-contracts'

type LogCategoryProps = {
	category: LogCategory
}

export default function LogCategoryEl({ category }: LogCategoryProps) {
	return <Tag>{getLogCategodyName(category)}</Tag>
}
