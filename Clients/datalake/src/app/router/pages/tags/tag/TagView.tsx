import BlockButton from '@/app/components/buttons/BlockButton'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import CopyableText from '@/app/components/CopyableText'
import TagIcon from '@/app/components/icons/TagIcon'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import PageHeader from '@/app/components/PageHeader'
import TabsView from '@/app/components/tabsView/TabsView'
import TagResolutionEl from '@/app/components/TagResolutionEl'
import { encodeBlockTagPair, TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagTypeEl from '@/app/components/TagTypeEl'
import TagsValuesViewer from '@/app/components/values/TagsValuesViewer'
import routes from '@/app/router/routes'
import {
	AccessType,
	BlockTagRelation,
	SourceType,
	TagAggregation,
	TagFullInfo,
	TagResolution,
} from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { Button, Spin, Table, Tag } from 'antd'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import TagFormulaView from './views/TagFormulaView'
import TagThresholdsView from './views/TagThresholdsView'

const TagView = observer(() => {
	const store = useAppStore()
	const { id } = useParams()
	const navigate = useNavigate()
	const [tag, setTag] = useState({} as TagFullInfo)
	const [isLoading, setLoading] = useState(false)
	const [metrics, setMetrics] = useState({} as Record<string, string>)
	const hasLoadedRef = useRef(false)
	const lastIdRef = useRef<string | undefined>(id)

	// получение инфо
	const loadTagData = useCallback(() => {
		if (!id) return
		setLoading(true)

		Promise.all([
			store.api.inventoryTagsGet(Number(id)).then((res) => setTag(res.data)),
			store.api
				.dataTagsGetUsage({ tagsId: [Number(id)] })
				.then((res) => setMetrics(res.data[id]))
				.catch(() => setMetrics({})),
		]).finally(() => setLoading(false))
	}, [store.api, id])

	useEffect(() => {
		// Если изменился id, сбрасываем флаг загрузки
		if (lastIdRef.current !== id) {
			hasLoadedRef.current = false
			lastIdRef.current = id
		}

		if (hasLoadedRef.current || !id) return
		hasLoadedRef.current = true
		loadTagData()
	}, [loadTagData, id])

	const info: InfoTableProps['items'] = {
		Название: <CopyableText text={tag.name} />,
		Описание: tag.description == null ? <i>нет</i> : <pre>{tag.description}</pre>,
		Источник: <SourceButton source={{ id: tag.sourceId, name: tag.sourceName || '?' }} />,
		'Интервал обновления': tag.sourceId !== SourceType.Manual && tag.sourceId !== SourceType.Manual && (
			<TagResolutionEl resolution={tag.resolution} full={true} />
		),
	}

	if (tag.sourceId === SourceType.Aggregated) {
		info['Тип агрегирования'] = (
			<>
				{tag.aggregation === TagAggregation.Average ? 'Среднее' : 'Сумма'} за{' '}
				{tag.aggregationPeriod === TagResolution.Minute
					? 'прошедшую минуту'
					: tag.aggregationPeriod === TagResolution.Hour
						? 'прошедший час'
						: tag.aggregationPeriod === TagResolution.Day
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
		const mapping: TagMappingType = {}
		const rels: string[] = []

		// Основной тег
		const value = encodeBlockTagPair(0, tag.id) // Виртуальный ID для связи
		mapping[value] = {
			...tag,
			blockId: 0,
			localName: tag.name,
			relationType: BlockTagRelation.Static,
		}
		rels.push(value)

		return { tagMapping: mapping, relations: rels }
	}, [tag])

	const tabs = useMemo(() => {
		const _tabs = [
			{
				key: 'history',
				label: 'Значения',
				children: <TagsValuesViewer relations={relations} tagMapping={tagMapping} integrated={true} />,
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
							.filter((x) => CLIENT_REQUESTKEY != x.requestKey)}
					/>
				),
			},
		]
		if (tag.sourceId === SourceType.Calculated)
			_tabs.push({
				key: 'calc',
				label: 'Расчет',
				children: <TagFormulaView id={Number(id)} formula={tag.formula} inputs={tag.formulaInputs} />,
			})
		else if (tag.sourceId === SourceType.Thresholds) {
			_tabs.push({
				key: 'calc',
				label: 'Расчет',
				children: <TagThresholdsView tag={tag} />,
			})
		}
		return _tabs
	}, [tag, metrics, id, relations, tagMapping])

	return isLoading ? (
		<Spin />
	) : (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.tags.list)}>К списку тегов</Button>]}
				right={[
					store.hasAccessToTag(AccessType.Editor, tag.id) ? (
						<NavLink to={routes.tags.toEditTag(Number(id))}>
							<Button>Редактирование тега</Button>
						</NavLink>
					) : (
						<Button disabled>Редактирование тега</Button>
					),
				]}
				icon={<TagIcon type={tag.sourceType} />}
			>
				{tag.name}
			</PageHeader>

			<InfoTable items={info} />
			<br />
			<TabsView items={tabs} />
		</>
	)
})

export default TagView
