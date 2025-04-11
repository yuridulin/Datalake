import api from '@/api/swagger-api'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import TagTypeEl from '@/app/components/TagTypeEl'
import { user } from '@/state/user'
import { TagValue } from '@/types/tagValue'
import { Button, Spin } from 'antd'
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

	const info: InfoTableProps['items'] = {
		Название: tag.name,
		Описание: tag.description || <i>нет</i>,
		Источник: <SourceButton source={{ id: tag.sourceId, name: tag.sourceName || '?' }} />,
		'Интервал обновления': tag.sourceId !== SourceType.Manual && tag.sourceId !== SourceType.Manual && (
			<TagFrequencyEl frequency={tag.frequency} full={true} />
		),
	}

	if (tag.sourceId === SourceType.Calculated) {
		info['Формула'] = tag.formula
		info['Входные параметры'] = tag.formulaInputs.map((x) => (
			<div key={x.id}>
				{x.variableName} : <TagButton tag={x} />
			</div>
		))
	} else if (tag.sourceId === SourceType.Aggregated) {
		info['Тип агрегирования'] = (
			<>
				{tag.aggregation === TagAggregation.Average ? 'Среднее' : 'Сумма'} за{' '}
				{tag.aggregationPeriod === AggregationPeriod.Munite
					? 'прошедшую минуту'
					: tag.aggregationPeriod === AggregationPeriod.Hour
						? 'прошедший час'
						: tag.aggregationPeriod === AggregationPeriod.Day
							? 'прошедшие сутки'
							: '?'}
			</>
		)
		info['Тег-источник агрегирования'] = tag.sourceTag ? <TagButton tag={tag.sourceTag} /> : <i>нет</i>
	} else if (tag.sourceId > 0) {
		info['Путь к значению'] = <code>{tag.sourceItem}</code>
	}

	info['Тип данных'] = <TagTypeEl tagType={tag.type} />
	info['Текущее значение'] = <TagValueEl value={value} type={tag.type} />

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

			<InfoTable items={info} />
			<br />

			<LogsTableEl tagGuid={tag.guid} />
		</>
	)
})

export default TagView
