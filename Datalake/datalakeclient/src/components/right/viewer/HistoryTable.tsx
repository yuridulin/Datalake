import { HistoryResponse } from "../../../@types/HistoryResponse"
import { TagQualityDescription } from "../../../@types/enums/TagQuality"
import DateStr from "../../small/DateStr"
import TagValueEl from "../../small/TagValueEl"

export default function HistoryTable({ responses }: { responses: HistoryResponse[] }) {

	if (responses.length === 0) return <div>данные не получены</div>

	try {
		var dates = responses[0].Values.map((x, i) => ({ index: i, date: x.Date }))

		if (dates.length === 0) return <div>нет ни одной временной точки</div>

		var model = dates.map(d => ({
			date: d.date,
			values: responses.map(r => r.Values[d.index])
		}))

		return <div className="table">
			<div className="table-header">
				<span>Время</span>
				{responses.map((x, i) => <span key={i}>{x.TagName}</span>)}
			</div>
			{model.map((x, i) => <div className="table-row" key={i}>
				<span><DateStr date={x.date} /></span>
				{x.values.map((v, j) => <span key={j} title={TagQualityDescription(v.Quality)}><TagValueEl value={v.Value} /></span>)}
			</div>)}
		</div>
	}
	catch (e) {
		console.error(e)
		return <div>ошибка</div>
	}
}