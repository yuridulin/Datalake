import { BlockTagRelation } from '../../api/swagger/data-contracts'

type BlockTagRelationElProps = {
	relation: BlockTagRelation
}

export default function BlockTagRelationEl({ relation }: BlockTagRelationElProps) {
	return (
		<>
			{relation === BlockTagRelation.Input
				? 'входное'
				: relation === BlockTagRelation.Output
					? 'выходное'
					: 'сопутствующее'}
		</>
	)
}
