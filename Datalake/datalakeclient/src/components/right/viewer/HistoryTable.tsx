import dayjs from "dayjs"
import { HistoryResponse } from "../../../@types/HistoryResponse"
import { Tag } from "../../../@types/Tag"
import { TagQualityDescription } from "../../../@types/enums/TagQuality"
import TagQualityEl from "../../small/TagQualityEl"
import TagValueEl from "../../small/TagValueEl"

export default function HistoryTable({ responses, tags }: { responses: HistoryResponse[], tags: Tag[] }) {

	if (responses.length === 0) return <div>данные не получены</div>
	if (!tags || tags.length === 0) return <></>

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
				{responses.map((x, i) => {
					let tag = tags.filter(t => t.Id === x.Id)[0]
					return <span key={i} title={tag?.Description ?? ''}>{x.TagName}</span>
				})}
			</div>
			<div className="table-header">
				<span></span>
				{responses.map((x, i) => {
					let tag = tags.filter(t => t.Id === x.Id)[0]
					return <span key={i}>{tag?.Description ?? ''}</span>
				})}
			</div>
			{model.map((x, i) => <div className="table-row" key={i}>
				<span>{dayjs(x.date).format('DD.MM.YYYY hh:mm:ss')}</span>
				{x.values.map((v, j) => <span key={j} title={TagQualityDescription(v.Quality)}>
					<TagQualityEl quality={v.Quality} />
					&emsp;
					<TagValueEl value={v.Value} />
				</span>)}
			</div>)}
		</div>
	}
	catch (e) {
		console.error(e)
		return <div>ошибка</div>
	}
}