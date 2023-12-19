import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { Navigate } from "react-router-dom"
import { Button, ConfigProvider, DatePicker, Radio, Select } from "antd"
import axios from "axios"
import 'dayjs/locale/ru'
import locale from 'antd/locale/ru_RU'
import { PlayCircleOutlined } from "@ant-design/icons"
import { useInterval } from "../../../hooks/useInterval"
import { Tag } from "../../../@types/Tag"
import { HistoryRequest } from "../../../@types/HistoryRequest"
import { API } from "../../../router/api"
import { HistoryResponse } from "../../../@types/HistoryResponse"
import LiveTable from "./LiveTable"
import HistoryTable from "./HistoryTable"
import dayjs from "dayjs"

export default function Viewer() {

	const offset = new Date().getTimezoneOffset()
	const [ tags, setTags ] = useState([] as Tag[])
	const [ responses, setResponses ] = useState([] as HistoryResponse[])
	const [ options, setOptions ] = useState([] as { value: number, label: string }[])
	const [ live, setLive ] = useState(true)
	const [ settings, setSettings ] = useState({ 
		Tags: [] as number[],
		TagNames: [] as string[],
		Resolution: 0,
		Old: dayjs(dayjs(new Date().setDate(new Date().getDate() - 1)).format('DD.MM.YYYY'), 'DD.MM.YYYY').toDate(),
		Young: dayjs(dayjs(new Date()).format('DD.MM.YYYY'), 'DD.MM.YYYY').toDate(),
	} as HistoryRequest)

	const [ readTags, , error ] = useFetching(async () => {
		let res = await axios.get(API.tags.getFlatList)
		setTags(res.data)
		setOptions(res.data.map((x: Tag) => ({ value: x.Id, label: x.Name })))
	})

	const getValues = () => {
		if (settings.Tags && settings.Tags.length === 0) return console.log('Нечего спрашивать')
		loadValues()
	}

	const [ loadValues,, loadErr ] = useFetching(async () => {
		console.log(settings)
		let res = live 
			? await axios.post(API.tags.getLiveValues, {
				request: {
					Tags: settings.Tags
				}
			})
			: await axios.post(API.tags.getHistoryValues, { 
				request: [{
					Tags: settings.Tags,
					Old: dayjs(settings.Old).add(0-offset, 'minute').toISOString(),
					Young: dayjs(settings.Young).add(0-offset, 'minute').toISOString(),
					Resolution: settings.Resolution,
				}]
			})
		setResponses(res.data as HistoryResponse[])
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { readTags() }, [])
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { getValues() }, [settings, live])
	useInterval(() => { if (live) getValues() }, 1000)

	return (
		error
		? <Navigate to="/offline" />
		: tags.length === 0
			? <div><i>не создано ни одного тега</i></div> 
			: <>
			<div>
				<Select 
					mode="tags"
					style={{ width: '100%' }}
					onChange={values => setSettings({ ...settings, Tags: values })}
					tokenSeparators={[',', ';', ' ']}
					placeholder="Выберите теги для просмотра..."
					options={options}
				/>
			</div>
			<br />
			<div>
				<Radio.Group value={live} onChange={e => setLive(e.target.value)}>
					<Radio.Button value={true}>Текущие</Radio.Button>
					<Radio.Button value={false}>Архивные</Radio.Button>
				</Radio.Group>

				<div style={{ display: live ? 'none' : 'inline' }}>
					&emsp;
					<ConfigProvider locale={locale} >
						<DatePicker.RangePicker
							showTime={{ format: 'HH:mm' }}
							defaultValue={[dayjs(settings.Old), dayjs(settings.Young)]}
							format="DD.MM.YYYY HH:mm"
							placeholder={['Начало', 'Конец']}
							onChange={values => setSettings({ 
								...settings,
								Old: values?.[0]?.toDate() ?? new Date(),
								Young: values?.[1]?.toDate() ?? new Date()
							})}
						/>
					</ConfigProvider>
					&emsp;
					<Select
						style={{ width: '12em' }}
						value={settings.Resolution}
						onChange={v => setSettings({ ...settings, Resolution: v })}
						options={[
							{ value: 0, label: 'по изменению' },
							{ value: 1000, label: 'по секундам' },
							{ value: 60000, label: 'по минутам' },
							{ value: 360000, label: 'по часам' },
							{ value: 86400000, label: 'по суткам' },
						]}
					></Select>
					&emsp;
					<Button icon={<PlayCircleOutlined />} onClick={loadValues}></Button>
				</div>
			</div>
			<br />
			{loadErr
			? <div>Ошибка</div>
			: live 
				? <LiveTable responses={responses} tags={tags} />
				: <HistoryTable responses={responses} tags={tags} />}
		</>
	)
}