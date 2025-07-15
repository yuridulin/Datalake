import api from '@/api/swagger-api'
import BlockButton from '@/app/components/buttons/BlockButton'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import CopyableText from '@/app/components/CopyableText'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import TabsView from '@/app/components/tabsView/TabsView'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import TagTypeEl from '@/app/components/TagTypeEl'
import { TagValuesViewer } from '@/app/components/values/TagValuesViewer'
import TagFormulaView from '@/app/pages/tags/tag/views/TagFormulaView'
import { user } from '@/state/user'
import { INNER_REQUESTS } from '@/types/constants'
import { Button, Spin, Table, Tag } from 'antd'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import {
	AccessType,
	AggregationPeriod,
	SourceType,
	TagAggregation,
	TagFullInfo,
	TagType,
} from '../../../../api/swagger/data-contracts'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

const TagView = observer(() => {
	const { id } = useParams()
	const navigate = useNavigate()
	const [tag, setTag] = useState({} as TagFullInfo)
	const [isLoading, setLoading] = useState(false)
	const [metrics, setMetrics] = useState({} as Record<string, string>)

	// получение инфы
	const loadTagData = useCallback(() => {
		if (!id) return
		setLoading(true)

		Promise.all([
			api.tagsRead(Number(id)).then((res) => setTag(res.data)),
			api
				.systemGetTagState(Number(id))
				.then((res) => setMetrics(res.data))
				.catch(() => setMetrics({})),
		]).finally(() => setLoading(false))
	}, [id])

	useEffect(loadTagData, [loadTagData])

	const info: InfoTableProps['items'] = {
		Название: <CopyableText text={tag.name} />,
		Описание: tag.description == null ? <i>нет</i> : <pre>{tag.description}</pre>,
		Источник: <SourceButton source={{ id: tag.sourceId, name: tag.sourceName || '?' }} />,
		'Интервал обновления': tag.sourceId !== SourceType.Manual && tag.sourceId !== SourceType.Manual && (
			<TagFrequencyEl frequency={tag.frequency} full={true} />
		),
	}

	if (tag.sourceId === SourceType.Calculated) {
		info['Формула'] = tag.formula
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

	const { tagMapping, relations } = useMemo(() => {
		const mapping: Record<number, { id: number; localName: string; type: TagType }> = {}
		const rels: number[] = []

		// Основной тег
		const mainRelId = tag.id * -1 // Виртуальный ID для связи
		mapping[mainRelId] = {
			id: tag.id,
			localName: tag.name,
			type: tag.type,
		}
		rels.push(mainRelId)

		// Входные теги (для расчетных)
		/* if (tag.sourceId === SourceType.Calculated) {
			tag.formulaInputs?.forEach((input) => {
				const inputRelId = input.id * -1 // Виртуальный ID
				mapping[inputRelId] = {
					id: input.id,
					localName: input.variableName,
					type: input.type,
				}
				rels.push(inputRelId)
			})
		} */

		return { tagMapping: mapping, relations: rels }
	}, [tag])

	const tabs = useMemo(() => {
		const _tabs = [
			{
				key: 'history',
				label: 'Значения',
				children: <TagValuesViewer relations={relations} tagMapping={tagMapping} integrated={true} />,
			},
			{
				key: 'blocks',
				label: 'Блоки',
				children:
					!tag.blocks || tag.blocks.length === 0 ? (
						<i>нет</i>
					) : (
						<Table
							size='small'
							dataSource={tag.blocks}
							rowKey={'relationId'}
							columns={[
								{
									key: 'block',
									title: 'Блок',
									dataIndex: 'id',
									width: '40%',
									render: (_, block) => <BlockButton block={block} />,
								},
								{
									key: 'name',
									title: 'Название в блоке',
									dataIndex: 'localName',
								},
							]}
						/>
					),
			},
			{
				key: 'logs',
				label: 'События',
				children: <LogsTableEl tagGuid={tag.guid} />,
			},
			{
				key: 'metrics',
				label: 'Запросы',
				children: (
					<Table
						size='small'
						rowKey={'requestKey'}
						columns={[
							{
								key: 'date',
								dataIndex: 'date',
								title: 'Последнее обращение',
								width: '14em',
							},
							{
								key: 'requestKey',
								dataIndex: 'requestKey',
								title: 'Идентификатор запроса',
								render: (key) => {
									switch (key) {
										case 'calculate-collector':
											return (
												<>
													<Tag>внутренний</Tag> вычислитель
												</>
											)
										case 'aggregate-collector-min':
										case 'aggregate-collector-hour':
										case 'aggregate-collector-day':
											return (
												<>
													<Tag>внутренний</Tag> агрегатор
												</>
											)
										default:
											return <>{key}</>
									}
								},
							},
						]}
						dataSource={Object.entries(metrics)
							.map(([requestKey, date]) => ({
								date: dayjs(date).format('YYYY-MM-DD HH:mm:ss'),
								requestKey: requestKey,
							}))
							// внутренние запросы, игнорируем их
							.filter((x) => !INNER_REQUESTS.includes(x.requestKey))}
					/>
				),
			},
		]
		if (tag.sourceId === SourceType.Calculated)
			_tabs.push({
				key: 'calc',
				label: 'Расчет',
				children: <TagFormulaView formula={tag.formula} inputs={tag.formulaInputs} />,
			})
		return _tabs
	}, [tag, metrics, relations, tagMapping])

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
						{user.hasAccessToTag(AccessType.Editor, tag.id) ? (
							<NavLink to={routes.tags.toEditTag(Number(id))}>
								<Button>Редактирование тега</Button>
							</NavLink>
						) : (
							<Button disabled>Редактирование тега</Button>
						)}
					</>
				}
			>
				Тег {tag.name}
			</PageHeader>

			<InfoTable items={info} />
			<br />
			<TabsView items={tabs} />
		</>
	)
})

export default TagView
