import TagButton from '@/app/components/buttons/TagButton'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import TagCompactValue from '@/app/components/values/TagCompactValue'
import { TagFullInfo, TagQuality, TagThresholdInfo, TagType, ValueRecord } from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { DollarCircleOutlined } from '@ant-design/icons'
import { Space, Table } from 'antd'
import { ColumnsType } from 'antd/es/table'
import { useCallback, useState } from 'react'

interface TagThresholdsViewProps {
	tag: TagFullInfo
}

type TagThresholdsValues = Record<number, ValueRecord>

const TagThresholdsView = ({ tag }: TagThresholdsViewProps) => {
	const store = useAppStore()
	const [values, setValues] = useState<TagThresholdsValues>({})

	const getValues = useCallback(async () => {
		const res = await store.api.valuesGet([
			{ requestKey: CLIENT_REQUESTKEY, tagsId: [tag.id, tag.thresholdSourceTag?.id ?? 0] },
		])
		const newValues: TagThresholdsValues = {}
		res.data[0].tags.forEach((x) => {
			newValues[x.id] = x.values[0]
		})
		setValues(newValues)
	}, [store.api, tag])

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
			<PollingLoader pollingFunction={getValues} interval={5000} />
			<Table size='small' dataSource={tag.thresholds ?? []} columns={columns} />
		</>
	)
}

export default TagThresholdsView
