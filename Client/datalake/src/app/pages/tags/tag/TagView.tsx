import api from '@/api/swagger-api'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import TagTypeEl from '@/app/components/TagTypeEl'
import { user } from '@/state/user'
import { TagValue } from '@/types/tagValue'
import { Button, Descriptions, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import {
	AccessType,
	AggregationPeriod,
	SourceType,
	TagAggregation,
	TagInfo,
} from '../../../../api/swagger/data-contracts'
import { useInterval } from '../../../../hooks/useInterval'
import PageHeader from '../../../components/PageHeader'
import TagValueEl from '../../../components/TagValueEl'
import routes from '../../../router/routes'

const TagView = observer(() => {
	const { id } = useParams()
	const navigate = useNavigate()

	const [tag, setTag] = useState({} as TagInfo)
	const [value, setValue] = useState(null as TagValue)
	const [isLoading, setLoading] = useState(false)

	// получение инфы
	const loadTagData = () => {
		if (!id) return
		setLoading(true)

		api
			.tagsRead(String(id))
			.then((res) => {
				setTag(res.data)
			})
			.finally(() => setLoading(false))
	}

	useEffect(loadTagData, [id])

	const getValue = useCallback(() => {
		if (!id) return
		setValue((prevValue) => {
			api
				.valuesGet([
					{
						requestKey: 'tag-current-value',
						tags: [String(id)],
					},
				])
				.then((res) => setValue(res.data[0].tags[0].values[0].value))
				.catch(() => setValue(null))
			return prevValue
		})
	}, [id])

	useEffect(getValue, [tag, getValue])
	useInterval(getValue, 1000)

	return isLoading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={
					<>
						<Button onClick={() => navigate(routes.tags.list)}>К списку тегов</Button>
					</>
				}
				right={
					<>
						{user.hasAccessToTag(AccessType.Editor, tag.guid) && (
							<NavLink to={routes.tags.toEditTag(String(id))}>
								<Button>Редактирование тега</Button>
							</NavLink>
						)}
					</>
				}
			>
				Тег {tag.name}
			</PageHeader>

			<Descriptions bordered size={'small'} column={1} labelStyle={{ width: '250px' }}>
				<Descriptions.Item label='Название'>{tag.name}</Descriptions.Item>
				<Descriptions.Item label='Описание'>{tag.description || <i>нет</i>}</Descriptions.Item>
				<Descriptions.Item label='Источник'>
					<SourceButton source={{ id: tag.sourceId, name: tag.sourceName || '?' }} />
				</Descriptions.Item>
				{tag.sourceId !== SourceType.Manual && tag.sourceId !== SourceType.Manual && (
					<Descriptions.Item label='Интервал обновления'>
						<TagFrequencyEl frequency={tag.frequency} full={true} />
					</Descriptions.Item>
				)}
				{tag.sourceId === SourceType.Calculated ? (
					<>
						<Descriptions.Item label='Формула'>{tag.formula}</Descriptions.Item>
						<Descriptions.Item label='Входные параметры'>
							{tag.formulaInputs.map((x) => (
								<div key={x.id}>
									{x.variableName} : <TagButton tag={x} />
								</div>
							))}
						</Descriptions.Item>
					</>
				) : tag.sourceId === SourceType.Aggregated && tag.sourceTag ? (
					<>
						<Descriptions.Item label='Тип агрегирования'>
							{tag.aggregation === TagAggregation.Average ? 'Среднее' : 'Сумма'} за{' '}
							{tag.aggregationPeriod === AggregationPeriod.Munite
								? 'прошедшую минуту'
								: tag.aggregationPeriod === AggregationPeriod.Hour
									? 'прошедший час'
									: tag.aggregationPeriod === AggregationPeriod.Day
										? 'прошедшие сутки'
										: '?'}
						</Descriptions.Item>
						<Descriptions.Item label='Тег-источник агрегирования'>
							<TagButton tag={tag.sourceTag} />
						</Descriptions.Item>
					</>
				) : tag.sourceId > 0 ? (
					<>
						<Descriptions.Item label='Путь к значению'>
							<code>{tag.sourceItem}</code>
						</Descriptions.Item>
					</>
				) : (
					<></>
				)}
				<Descriptions.Item label='Тип данных'>
					<TagTypeEl tagType={tag.type} />
				</Descriptions.Item>
				<Descriptions.Item label='Текущее значение'>
					<TagValueEl value={value} type={tag.type} />
				</Descriptions.Item>
			</Descriptions>
			<br />
			<LogsTableEl tagGuid={tag.guid} />
		</>
	)
})

export default TagView
