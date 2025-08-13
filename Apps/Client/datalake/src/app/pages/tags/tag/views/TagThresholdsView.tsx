import api from '@/api/swagger-api'
import { TagFullInfo, TagQuality, TagThresholdInfo, TagType, ValueRecord } from '@/api/swagger/data-contracts'
import TagButton from '@/app/components/buttons/TagButton'
import TagCompactValue from '@/app/components/TagCompactValue'
import { useInterval } from '@/hooks/useInterval'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { DollarCircleOutlined } from '@ant-design/icons'
import { Space, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useEffect, useState } from 'react'

interface TagThresholdsViewProps {
	tag: TagFullInfo
}

type TagThresholdsValues = Record<number, ValueRecord>

const TagThresholdsView = ({ tag }: TagThresholdsViewProps) => {
	const [values, setValues] = useState<TagThresholdsValues>({})

	const getValues = () => {
		api
			.valuesGet([{ requestKey: CLIENT_REQUESTKEY, tagsId: [tag.id, tag.thresholdSourceTag?.id ?? 0] }])
			.then((res) => {
				const newValues: TagThresholdsValues = {}
				res.data[0].tags.forEach((x) => {
					newValues[x.id] = x.values[0]
				})
				setValues(newValues)
			})
	}

	useEffect(getValues, [getValues])
	useInterval(getValues, 5000)

	const columns: ColumnsType<TagThresholdInfo> = [
		{
			key: '1',
			render(_, record) {
				const current = values[tag.id]
				return current && record.result == current.value ? <DollarCircleOutlined /> : <></>
			},
		},
		{
			dataIndex: 'threshold',
			title: 'Входное значение',
			render(value) {
				return <TagCompactValue value={value} quality={TagQuality.GoodLOCF} type={TagType.Number} />
			},
		},
		{
			dataIndex: 'result',
			title: 'Результирующее значение',
			render(value) {
				return <TagCompactValue value={value} quality={TagQuality.GoodLOCF} type={TagType.Number} />
			},
		},
	]

	const InputValue = () => {
		if (!tag.thresholdSourceTag) return <></>
		const inputValue = values[tag.thresholdSourceTag.id]
		if (!inputValue) return <></>

		return <TagCompactValue type={tag.thresholdSourceTag.type} value={inputValue.value} quality={inputValue.quality} />
	}

	return (
		<>
			<Space>
				Тег-источник:{' '}
				{tag.thresholdSourceTag ? (
					<>
						<TagButton tag={tag.thresholdSourceTag} /> Значение: <InputValue />
					</>
				) : (
					<>не задан</>
				)}
			</Space>
			<br />
			<br />
			<Table size='small' dataSource={tag.thresholds ?? []} columns={columns} />
		</>
	)
}

export default TagThresholdsView
