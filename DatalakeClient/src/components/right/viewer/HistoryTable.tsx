import dayjs from 'dayjs'
import {
	TagInfo,
	TagQuality,
	ValuesResponse,
} from '../../../api/swagger/data-contracts'
import TagQualityEl from '../../small/TagQualityEl'
import TagValueEl from '../../small/TagValueEl'

function TagQualityDescription(quality: TagQuality) {
	switch (quality) {
		case TagQuality.BadManualWrite:
			return 'не достоверно, ручной ввод'
		case TagQuality.BadNoConnect:
			return 'не достоверно, нет связи'
		case TagQuality.BadNoValues:
			return 'не достоверно, нет значения'
		case TagQuality.Good:
			return 'достоверно'
		case TagQuality.GoodManualWrite:
			return 'достоверно, ручной ввод'
		default:
			return 'не достоверно'
	}
}

export default function HistoryTable({
	responses,
	tags,
}: {
	responses: ValuesResponse[]
	tags: TagInfo[]
}) {
	const offset = new Date().getTimezoneOffset()
	if (responses.length === 0) return <div>данные не получены</div>
	if (!tags || tags.length === 0) return <></>

	try {
		var dates = responses[0].values.map((x, i) => ({
			index: i,
			date: x.date,
		}))

		if (dates.length === 0) return <div>нет ни одной временной точки</div>

		var model = dates.map((d) => ({
			date: d.date,
			values: responses.map((r) => r.values[d.index]),
		}))

		return (
			<div className='table'>
				<div className='table-header'>
					<span>Время</span>
					{responses.map((x, i) => {
						let tag = tags.filter((t) => t.id === x.id)[0]
						return (
							<span key={i} title={tag.description ?? ''}>
								{x.tagName}
							</span>
						)
					})}
				</div>
				<div className='table-header'>
					<span></span>
					{responses.map((x, i) => {
						let tag = tags.filter((t) => t.id === x.id)[0]
						return <span key={i}>{tag.description ?? ''}</span>
					})}
				</div>
				{model.map((x, i) => (
					<div className='table-row' key={i}>
						<span>
							{dayjs(x.date)
								.add(offset, 'minutes')
								.format('DD.MM.YYYY HH:mm:ss')}
						</span>
						{x.values.map((v, j) => (
							<span
								key={j}
								title={TagQualityDescription(v.quality)}
							>
								<TagQualityEl quality={v.quality} />
								&emsp;
								<TagValueEl value={v.value} />
							</span>
						))}
					</div>
				))}
			</div>
		)
	} catch (e) {
		console.error(e)
		return <div>ошибка</div>
	}
}
