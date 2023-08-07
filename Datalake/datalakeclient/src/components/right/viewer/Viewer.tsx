import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import { Navigate } from "react-router-dom"
import { Button, ConfigProvider, DatePicker, Radio, Select } from "antd"
import axios from "axios"
import 'dayjs/locale/ru';
import locale from 'antd/locale/ru_RU'
import { PlayCircleOutlined } from "@ant-design/icons"
import { useInterval } from "../../../hooks/useInterval"
import Quality from "../../small/Quality"
import TagValueElement from "../../small/TagValue"
import { Tag } from "../../../@types/Tag"
import { TagValue } from "../../../@types/TagValue"

export default function Viewer() {

	const [ tags, setTags ] = useState([] as Tag[])
	const [ values, setValues ] = useState([] as TagValue[])
	const [ options, setOptions ] = useState([] as { value: number, label: string }[])
	const [ settings, setSettings ] = useState({ tags: [], live: true, resolution: 0, young: new Date(), old: new Date() })

	const [ readTags, , error ] = useFetching(async () => {
		let res = await axios.get('tags/list')
		setTags(res.data)
		setOptions(res.data.map((x: Tag) => ({ value: x.Id, label: x.Name })))
	})

	const getValues = () => {
		if (settings.tags.length === 0) return console.log('Нечего спрашивать')
		if (settings.live || (settings.young >= new Date())) {
			loadValues()
		}
		else {
			console.log('Незачем спрашивать, вся история отдана')
		}
	}

	const [ loadValues ] = useFetching(async () => {
		let res = await axios.post('tags/' + (settings.live ? 'live' : 'history'), settings)
		setValues(res.data)
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { readTags() }, [])
	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { getValues() }, [settings])
	useInterval(() => { if (settings.live) getValues() }, 1000)

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
					onChange={values => setSettings({ ...settings, tags: values })}
					tokenSeparators={[',', ';', ' ']}
					placeholder="Выберите теги для просмотра..."
					options={options}
				/>
			</div>
			<br />
			<div>
				<Radio.Group value={settings.live} onChange={e => setSettings({ ...settings, live: e.target.value })}>
					<Radio.Button value={true}>Текущие</Radio.Button>
					<Radio.Button value={false}>Архивные</Radio.Button>
				</Radio.Group>

				<div style={{ display: settings.live ? 'none' : 'inline' }}>
					&emsp;
					<ConfigProvider locale={locale}>
						<DatePicker.RangePicker
							showTime={{ format: 'HH:mm' }}
							format="YYYY-MM-DD HH:mm"
							placeholder={['Начало', 'Конец']}
							onChange={values => setSettings({ ...settings, old: values?.[0]?.toDate() ?? new Date(), young: values?.[1]?.toDate() ?? new Date() })}
						/>
					</ConfigProvider>
					&emsp;
					<Select
						style={{ width: '12em' }}
						value={settings.resolution}
						onChange={v => setSettings({ ...settings, resolution: v })}
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
			{settings.tags.length > 0 &&
			<div className="table">
				<div className="table-header">
					<span>Время</span>
					<span>Тег</span>
					<span>Значение</span>
					<span>Качество</span>
				</div>
				{values.map((x, i) => <div className="table-row" key={i}>
					<span>{x.Date.toString()}</span>
					<span>{x.TagName}</span>
					<span><TagValueElement value={x.Value} /></span>
					<span><Quality quality={x.Quality} /></span>
				</div>)}
			</div>}
		</>
	)
}