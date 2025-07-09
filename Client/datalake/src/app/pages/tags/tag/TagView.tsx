import api from '@/api/swagger-api'
import BlockButton from '@/app/components/buttons/BlockButton'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import CopyableText from '@/app/components/CopyableText'
import InfoTable, { InfoTableProps } from '@/app/components/infoTable/InfoTable'
import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import TabsView from '@/app/components/tabsView/TabsView'
import TagCompactValue from '@/app/components/TagCompactValue'
import TagFrequencyEl from '@/app/components/TagFrequencyEl'
import TagTypeEl from '@/app/components/TagTypeEl'
import TagValueText from '@/app/components/TagValue'
import { user } from '@/state/user'
import { Button, Spin, Table, Tag } from 'antd'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { NavLink, useNavigate, useParams } from 'react-router-dom'
import {
	AccessType,
	AggregationPeriod,
	SourceType,
	TagAggregation,
	TagFullInfo,
	TagQuality,
	ValueRecord,
} from '../../../../api/swagger/data-contracts'
import { useInterval } from '../../../../hooks/useInterval'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'

const TagView = observer(() => {
	const { id } = useParams()
	const navigate = useNavigate()

	const [tag, setTag] = useState({} as TagFullInfo)
	const [isLoading, setLoading] = useState(false)

	const [viewTags, setViewTags] = useState([] as number[])
	const [viewValues, setViewValues] = useState({} as Record<number, ValueRecord>)
	const [thisTagValue, setThisTagValue] = useState({} as ValueRecord)
	const [metrics, setMetrics] = useState({} as Record<string, string>)

	// получение инфы
	const getMetrics = useCallback(() => {
		api
			.systemGetTagState(Number(id))
			.then((res) => {
				setMetrics(res.data)
			})
			.catch(() => {
				setMetrics({})
			})
	}, [id])

	const loadTagData = () => {
		if (!id) return
		setLoading(true)

		api
			.tagsRead(Number(id))
			.then((res) => {
				setTag(res.data)
				if (res.data.sourceId === SourceType.Calculated) {
					setViewTags([res.data.id, ...res.data.formulaInputs.map((x) => x.id)])
				} else {
					setViewTags([res.data.id])
				}
				getMetrics()
			})
			.finally(() => setLoading(false))
	}

	useEffect(loadTagData, [id, getMetrics])

	const getViewValues = useCallback(() => {
		if (!id) return

		api
			.valuesGet([
				{
					requestKey: 'tag-current-value',
					tagsId: viewTags,
				},
			])
			.then((res) => {
				const thisId = Number(id)
				const newViewValues = {} as Record<number, ValueRecord>
				res.data[0].tags.forEach((tag) => {
					newViewValues[tag.id] = tag.values.length
						? tag.values[0]
						: { date: '', dateString: '', quality: TagQuality.BadNoValues, value: null }
					if (tag.id === thisId) setThisTagValue(newViewValues[tag.id])
				})
				setViewValues(newViewValues)
			})
			.catch(() => {
				setViewValues({})
				setThisTagValue({} as ValueRecord)
			})
	}, [id, viewTags])

	const renderFormulaWithValues = (formula: string) => {
		if (!formula) return formula
		if (!tag.formulaInputs.length) return formula

		// Функция экранирования для RegExp
		const escapeRegExp = (str: string) => str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')

		// Подготовка вариантов для поиска: [name] и name
		const allPatterns: string[] = []
		tag.formulaInputs.forEach((input) => {
			allPatterns.push(`\\[${escapeRegExp(input.variableName)}\\]`)
			allPatterns.push(escapeRegExp(input.variableName))
		})

		// Сортировка по убыванию длины для приоритета длинных совпадений
		allPatterns.sort((a, b) => b.length - a.length)
		const regex = new RegExp(`(${allPatterns.join('|')})`)
		const parts = formula.split(regex)

		return parts.map((part, index) => {
			// Проверяем все возможные форматы параметра
			let variableName: string | null = null

			// Вариант 1: в квадратных скобках [name]
			if (part.startsWith('[') && part.endsWith(']')) {
				variableName = part.slice(1, -1).trim()
			}
			// Вариант 2: без скобок (name)
			else {
				variableName = part.trim()
			}

			// Ищем соответствующий входной параметр
			const input = tag.formulaInputs?.find((x) => x.variableName === variableName)

			if (input) {
				const valueInfo = viewValues[input.id]
				let valueContent = <>?</>

				if (valueInfo) {
					valueContent = <TagValueText type={input.type} value={valueInfo.value} />
				}

				return valueContent
			}

			// Возвращаем обычный текст для не-параметров
			return <span key={index}>{part}</span>
		})
	}

	useEffect(getViewValues, [tag, getViewValues])
	useInterval(getViewValues, 1000)

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
		info['Формула со значениями'] = renderFormulaWithValues(tag.formula ?? '')

		info['Входные параметры'] =
			tag.formulaInputs.length > 0 ? (
				<Table
					size='small'
					bordered
					indentSize={1}
					pagination={false}
					dataSource={tag.formulaInputs}
					columns={[
						{
							key: 'name',
							dataIndex: 'variableName',
							title: 'Обозначение',
							width: '10em',
						},
						{
							key: 'value',
							title: 'Значение',
							width: '10em',
							render: (_, x) => {
								const value = viewValues[x.id]
								return value ? <TagCompactValue type={x.type} quality={value.quality} value={value.value} /> : <>?</>
							},
						},
						{
							key: 'link',
							title: 'Используемый тег',
							render: (_, x) => <TagButton tag={x} />,
						},
					]}
				/>
			) : (
				<>не заданы</>
			)
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

	info['Текущее значение'] = (
		<TagCompactValue value={thisTagValue.value} type={tag.type} quality={thisTagValue.quality} />
	)

	const tabs = [
		{
			key: 'blocks',
			label: 'Блоки с этим тегом',
			children:
				!tag.blocks || tag.blocks.length === 0 ? (
					<i>нет</i>
				) : (
					<Table
						size='small'
						dataSource={tag.blocks}
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
	]

	if (user.hasGlobalAccess(AccessType.Admin))
		tabs.push({
			key: 'metrics',
			label: 'Обращения к этому тегу',
			children: (
				<Table
					size='small'
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
						.filter((x) => !['block-values', 'tag-current-value', 'tags-table'].includes(x.requestKey))}
				/>
			),
		})

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
