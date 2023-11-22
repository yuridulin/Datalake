import { HistoryResponse } from "../../../@types/HistoryResponse"
import DateStr from "../../small/DateStr"
import TagQualityEl from "../../small/TagQualityEl"
import TagValueEl from "../../small/TagValueEl"

export default function LiveTable({ responses }: { responses: HistoryResponse[] }) {
	return responses.length > 0
		? <div className="table">
			<div className="table-header">
				<span>Время</span>
				<span>Тег</span>
				<span>Значение</span>
				<span>Качество</span>
			</div>
			{responses.map((x, i) => <div className="table-row" key={i}>
				<span><DateStr date={x.Values[0]?.Date}/></span>
				<span>{x.TagName}</span>
				<span><TagValueEl value={x.Values[0]?.Value} /></span>
				<span><TagQualityEl quality={x.Values[0]?.Quality} /></span>
			</div>)}
		</div>
		: <div></div>
}