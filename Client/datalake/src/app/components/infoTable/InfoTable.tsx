import { Descriptions } from 'antd'
import { ReactNode } from 'react'

export type InfoTableProps = {
	items: Record<string, string | ReactNode>
}

type InfoTableItems = { label: string; children: string | ReactNode }[]

const InfoTable = ({ items }: InfoTableProps) => {
	const descriptions = Object.entries(items).reduce(
		(acc, next) => (next[1] ? [...acc, { label: next[0], children: next[1] }] : acc),
		[] as InfoTableItems,
	)

	return <Descriptions bordered size={'small'} column={1} labelStyle={{ width: '250px' }} items={descriptions} />
}

export default InfoTable
