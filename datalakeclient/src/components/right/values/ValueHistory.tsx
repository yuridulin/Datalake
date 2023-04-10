import { useEffect, useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import valuesApi from "../../../api/valuesApi"
import { Navigate, useParams } from "react-router-dom"
import { Button, DatePicker, Select } from "antd"
import dayjs from "dayjs"
import locale from 'antd/es/date-picker/locale/ru_RU'
import { ValueRange } from "../../../@types/valueRange"

export default function ValueHistory() {

	const dateFormat = 'DD.MM.YYYY HH:mm:ss'

	const { tagName } = useParams()
	const [ form, setForm ] = useState({ 
		old: dayjs(new Date()).add(-10, 'minute').format(dateFormat),
		young: dayjs(new Date()).format(dateFormat),
		resolution: 0
	})
	const [ range, setRange ] = useState({ TagName: '', Values: [] } as ValueRange)

	const resolutions = [
		{ value: 0, label: 'по изменению' },
		{ value: 1000, label: 'по секундам' },
		{ value: 60000, label: 'по минутам' },
	]

	const [ load, , error ] = useFetching(async () => {
		let data = await valuesApi.history({ tags: [tagName || ''], ...form })
		if (data) setRange(data[0])
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { load() }, [])

	return (
		error
		?
		<Navigate to="/offline" />
		:
		<div>
			<div>Тег {tagName}</div>
			<div>
				<span style={{ marginRight: '.5em' }}>История с</span>
				<DatePicker
					showTime
					locale={locale}
					format={dateFormat}
					defaultValue={dayjs(form.old)}
					onOk={e => setForm({ ...form, old: e.format(dateFormat) })}
				/>
				<span style={{ marginRight: '.5em', marginLeft: '.5em' }}>по</span>
				<DatePicker
					showTime
					locale={locale}
					format={dateFormat}
					defaultValue={dayjs(form.young)}
					onOk={e => setForm({ ...form, young: e.format(dateFormat) })}
				/>
				<Select
					style={{ marginRight: '.5em', marginLeft: '.5em', width: '10em' }}
					options={resolutions}
					value={form.resolution}
					onChange={e => setForm({ ...form, resolution: e })}
				></Select>
				<Button onClick={load}>Обновить</Button>
			</div>

			<table className="view-table">
				<thead>
					<tr>
						<th>Дата</th>
						<th>Строковое значение</th>
						<th>Числовое значение</th>
						<th>Качество</th>
					</tr>
				</thead>
				<tbody>
					{range.Values.map(x => 
					<tr key={x.Id}>
						<td>{x.Date}</td>
						<td>{x.Text}</td>
						<td>{x.Number}</td>
						<td>{x.Quality}</td>
					</tr>
					)}
				</tbody>
			</table>
		</div>
	)
}