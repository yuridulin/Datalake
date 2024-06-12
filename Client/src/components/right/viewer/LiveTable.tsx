import { Button } from 'antd'
import dayjs from 'dayjs'
import api from '../../../api/swagger-api'
import { TagInfo, ValuesResponse } from '../../../api/swagger/data-contracts'
import { CustomSource } from '../../../models/customSource'
import TagQualityEl from '../../small/TagQualityEl'
import TagValueEl from '../../small/TagValueEl'

export default function LiveTable({
	responses,
	tags,
}: {
	responses: ValuesResponse[]
	tags: TagInfo[]
}) {
	function write(tagId: number) {
		let value = prompt('Введите значение:', '')
		api.valuesWrite([{ tagId, value }])
	}

	return responses.length > 0 ? (
		<div className='table'>
			<div className='table-header'>
				<span>Время</span>
				<span>Тег</span>
				<span>Значение</span>
				<span>Качество</span>
				<span>Описание</span>
			</div>
			{responses.map((x, i) => {
				let tag = tags.filter((t) => t.id === x.id)[0]
				return (
					<div className='table-row' key={i}>
						<span>
							{dayjs(x.values[0].date).format(
								'DD.MM.YYYY hh:mm:ss',
							)}
						</span>
						<span>{x.tagName}</span>
						<span>
							{tag.sourceId === CustomSource.Manual ? (
								<Button onClick={() => write(tag.id)}>
									<TagValueEl value={x.values[0].value} />
								</Button>
							) : (
								<TagValueEl value={x.values[0].value} />
							)}
						</span>
						<span>
							<TagQualityEl quality={x.values[0].quality} />
						</span>
						<span>{tag.description ?? ''}</span>
					</div>
				)
			})}
		</div>
	) : (
		<div></div>
	)
}
