import { PlayCircleOutlined } from '@ant-design/icons'
import { Button, ConfigProvider, DatePicker, Radio, Select } from 'antd'
import locale from 'antd/locale/ru_RU'
import dayjs from 'dayjs'
import 'dayjs/locale/ru'
import { useEffect, useState } from 'react'
import { Navigate } from 'react-router-dom'
import api from '../../../api/swagger-api'
import {
	TagInfo,
	ValuesRequest,
	ValuesResponse,
} from '../../../api/swagger/data-contracts'
import { useFetching } from '../../../hooks/useFetching'
import HistoryTable from './HistoryTable'
import LiveTable from './LiveTable'

export default function Viewer() {
	const offset = new Date().getTimezoneOffset()
	const [tags, setTags] = useState([] as TagInfo[])
	const [responses, setResponses] = useState([] as ValuesResponse[])
	const [options, setOptions] = useState(
		[] as { value: number; label: string }[],
	)
	const [live, setLive] = useState(true)
	const [settings, setSettings] = useState({
		Tags: [] as number[],
		TagNames: [] as string[],
		Resolution: 0,
		Old: dayjs(
			dayjs(new Date().setDate(new Date().getDate() - 1)).format(
				'DD.MM.YYYY',
			),
			'DD.MM.YYYY',
		).toDate(),
		Young: dayjs(
			dayjs(new Date()).format('DD.MM.YYYY'),
			'DD.MM.YYYY',
		).toDate(),
	} as ValuesRequest)

	const [readTags, , error] = useFetching(async () => {
		api.tagsReadAll().then((res) => {
			setTags(res.data)
			setOptions(res.data.map((x) => ({ value: x.id, label: x.name })))
		})
	})

	const getValues = () => {
		if (settings.tags && settings.tags.length === 0)
			return console.log('Нечего спрашивать')
		loadValues()
	}

	const [loadValues, , loadErr] = useFetching(async () => {
		console.log(settings)
		let res = await api.valuesGet([
			live
				? { tags: settings.tags }
				: {
						tags: settings.tags,
						old: dayjs(settings.old)
							.add(0 - offset, 'minute')
							.toISOString(),
						young: dayjs(settings.young)
							.add(0 - offset, 'minute')
							.toISOString(),
						resolution: settings.resolution,
				  },
		])
		setResponses(res.data)
	})

	useEffect(() => {
		readTags()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [])
	useEffect(() => {
		getValues()
		const interval = setInterval(getValues, 5000)
		return () => clearInterval(interval)
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [settings, live])

	return error ? (
		<Navigate to='/offline' />
	) : tags.length === 0 ? (
		<div>
			<i>не создано ни одного тега</i>
		</div>
	) : (
		<>
			<div>
				<Select
					mode='tags'
					style={{ width: '100%' }}
					onChange={(values) =>
						setSettings({ ...settings, tags: values })
					}
					tokenSeparators={[',', ';', ' ']}
					placeholder='Выберите теги для просмотра...'
					options={options}
				/>
			</div>
			<br />
			<div>
				<Radio.Group
					value={live}
					onChange={(e) => setLive(e.target.value)}
				>
					<Radio.Button value={true}>Текущие</Radio.Button>
					<Radio.Button value={false}>Архивные</Radio.Button>
				</Radio.Group>

				<div style={{ display: live ? 'none' : 'inline' }}>
					&emsp;
					<ConfigProvider locale={locale}>
						<DatePicker.RangePicker
							showTime={{ format: 'HH:mm' }}
							defaultValue={[
								dayjs(settings.old),
								dayjs(settings.young),
							]}
							format='DD.MM.YYYY HH:mm'
							placeholder={['Начало', 'Конец']}
							onChange={(values) =>
								setSettings({
									...settings,
									old: values?.[0]?.toString(),
									young: values?.[1]?.toString(),
								})
							}
						/>
					</ConfigProvider>
					&emsp;
					<Select
						style={{ width: '12em' }}
						value={settings.resolution}
						onChange={(v) =>
							setSettings({ ...settings, resolution: v })
						}
						options={[
							{ value: 0, label: 'по изменению' },
							{ value: 1000, label: 'по секундам' },
							{ value: 60000, label: 'по минутам' },
							{ value: 360000, label: 'по часам' },
							{ value: 86400000, label: 'по суткам' },
						]}
					></Select>
					&emsp;
					<Button
						icon={<PlayCircleOutlined />}
						onClick={loadValues}
					></Button>
				</div>
			</div>
			<br />
			{loadErr ? (
				<div>Ошибка</div>
			) : live ? (
				<LiveTable responses={responses} tags={tags} />
			) : (
				<HistoryTable responses={responses} tags={tags} />
			)}
		</>
	)
}
