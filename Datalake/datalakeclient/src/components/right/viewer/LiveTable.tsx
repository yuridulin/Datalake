import dayjs from "dayjs"
import { HistoryResponse } from "../../../@types/HistoryResponse"
import { Tag } from "../../../@types/Tag"
import TagQualityEl from "../../small/TagQualityEl"
import TagValueEl from "../../small/TagValueEl"

export default function LiveTable({ responses, tags }: { responses: HistoryResponse[], tags: Tag[] }) {
	return responses.length > 0
		? <div className="table">
			<div className="table-header">
				<span>Время</span>
				<span>Тег</span>
				<span>Значение</span>
				<span>Качество</span>
				<span>Описание</span>
			</div>
			{responses.map((x, i) => {
				let tag = tags.filter(t => t.Id === x.Id)[0]
				return <div className="table-row" key={i}>
					<span>{dayjs(x.Values[0]?.Date).format('DD.MM.YYYY hh:mm:ss')}</span>
					<span>{x.TagName}</span>
					<span><TagValueEl value={x.Values[0]?.Value} /></span>
					<span><TagQualityEl quality={x.Values[0]?.Quality} /></span>
					<span>{tag?.Description ?? ''}</span>
				</div>
			})}
		</div>
		: <div></div>
}