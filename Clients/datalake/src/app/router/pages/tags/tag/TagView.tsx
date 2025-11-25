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
import { ALL_TAGS_ID, encodeBlockTagPair, TagMappingType } from '@/app/components/tagTreeSelect/treeSelectShared'
import TagTypeEl from '@/app/components/TagTypeEl'
import TagsValuesViewer from '@/app/components/values/TagsValuesViewer'
import routes from '@/app/router/routes'
import {
	AccessType,
	BlockTagRelation,
	SourceType,
	TagAggregation,
	TagResolution,
} from '@/generated/data-contracts'
import { useAppStore } from '@/store/useAppStore'
import { CLIENT_REQUESTKEY } from '@/types/constants'
import { Button, Spin, Table, Tag } from 'antd'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useEffect, useMemo, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import TagFormulaView from './views/TagFormulaView'
import TagThresholdsView from './views/TagThresholdsView'

const TagView = observer(() => {
	const store = useAppStore()
	const { id } = useParams()
	const navigate = useNavigate()
	const [metrics, setMetrics] = useState({} as Record<string, string>)

	// Получаем тег из store (реактивно через MobX)
	const tagId = id ? Number(id) : undefined
	const tag = tagId ? store.tagsStore.getTagById(tagId) : undefined
	const isLoading = tagId ? store.tagsStore.isLoadingTag(tagId) : false

	// Загружаем метрики использования тега
	useEffect(() => {
		if (!tagId) return

		store.api
			.dataTagsGetUsage({ tagsId: [tagId] })
			.then((res) => {
				const tagUsage = res.data?.find((item) => item.tagId === tagId)
				setMetrics(tagUsage?.requests ?? {})
			})
			.catch(() => setMetrics({}))
	}, [store.api, tagId])

	const info: InfoTableProps['items'] = useMemo(() => {
		if (!tag) return {}

		const items: InfoTableProps['items'] = {
			Название: <CopyableText text={tag.name} />,
			Описание: tag.description == null ? <i>нет</i> : <pre>{tag.description}</pre>,
			Источник: <SourceButton source={{ id: tag.sourceId, name: tag.sourceName || '?' }} />,
			'Интервал обновления':
				tag.sourceId !== SourceType.Manual && tag.sourceId !== SourceType.Calculated && (
					<TagResolutionEl resolution={tag.resolution} full={true} />
				),
		}

		if (tag.sourceId === SourceType.Aggregated) {
			items['Тип агрегирования'] = (
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
			items['Тег-источник агрегирования'] = tag.sourceTag ? <TagButton tag={tag.sourceTag} /> : <i>нет</i>
		} else if (tag.sourceId > 0) {
			items['Путь к значению'] = <code>{tag.sourceItem}</code>
		}

		items['Тип данных'] = <TagTypeEl tagType={tag.type} />

		return items
	}, [tag])

	const { tagMapping, relations } = useMemo(() => {
		if (!tag) return { tagMapping: {} as TagMappingType, relations: [] as string[] }

		const mapping: TagMappingType = {}
		const rels: string[] = []

		// Основной тег - создаем правильную структуру BlockNestedTagInfo
		// Используем ALL_TAGS_ID для совместимости с QueryTreeSelect
		const value = encodeBlockTagPair(ALL_TAGS_ID, tag.id)
		mapping[value] = {
			relationType: BlockTagRelation.Static,
			blockId: ALL_TAGS_ID,
			tagId: tag.id,
			localName: tag.name,
			tag: tag, // TagWithSettingsAndBlocksInfo расширяет TagSimpleInfo, поэтому совместим
		}
		rels.push(value)

		return { tagMapping: mapping, relations: rels }
	}, [tag])

	const tabs = useMemo(() => {
		if (!tag) return []

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
									render: (_, relation) =>
										relation.block ? <BlockButton block={relation.block} /> : <i>нет</i>,
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
				children: <LogsTableEl tagId={tag.id} />,
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
				children: <TagFormulaView id={tagId!} formula={tag.formula} inputs={tag.formulaInputs} />,
			})
		else if (tag.sourceId === SourceType.Thresholds) {
			_tabs.push({
				key: 'calc',
				label: 'Расчет',
				// TagWithSettingsAndBlocksInfo совместим с TagFullInfo для этого компонента
				children: <TagThresholdsView tag={tag as Parameters<typeof TagThresholdsView>[0]['tag']} />,
			})
		}
		return _tabs
	}, [tag, metrics, tagId, relations, tagMapping])

	if (!tagId) {
		return <Spin />
	}

	if (isLoading && !tag) {
		return <Spin />
	}

	if (!tag) {
		return <div>Тег не найден</div>
	}

	return (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.tags.list)}>К списку тегов</Button>]}
				right={[
					store.hasAccessToTag(AccessType.Editor, tag.id) ? (
						<NavLink to={routes.tags.toEditTag(tagId)}>
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
