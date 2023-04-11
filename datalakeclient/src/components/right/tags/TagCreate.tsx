import { Modal, Input, Select, InputNumber } from "antd"
import { useState, useEffect } from "react"
import { Source } from "../../../@types/source"
import sourcesApi from "../../../api/sourcesApi"
import tagsApi from "../../../api/tagsApi"
import { useFetching } from "../../../hooks/useFetching"
import { Tag } from "../../../@types/tag"

export default function TagCreate({ visible, setVisible, loadTable }: {
	visible: boolean
	setVisible: (x: boolean) => void
	loadTable: (x?: any) => void
}) {

	const [ form, setForm ] = useState({ SourceId: 0 } as Tag)

	const [ sources, setSources ] = useState([] as Source[])

	const [ create ] = useFetching(async () => {
		let res = await tagsApi.create(form)
		if (res.Done) {
			loadTable()
			setVisible(false)
		}
	})

	const [ getSources, isSourcesLoading ] = useFetching(async () => {
		let res = await sourcesApi.list()
		if (res) {
			setSources(res)
			console.log(sources)
		}
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { if (visible) getSources() }, [visible])

	return (
		<Modal
			title="Добавление нового тега"
			open={visible}
			okText="Добавить"
			onOk={create}
			cancelText="Отмена"
			onCancel={() => setVisible(false)}
		>
			<div className="form-caption">Имя</div>
			<Input
				value={form.TagName}
				onChange={e => setForm({ ...form, TagName: e.target.value })}
			/>
			
			<div className="form-caption">Тип</div>
			<Select
				options={[{ value: 0, label: 'строка' }, { value: 1, label: 'число' }, { value: 2, label: 'дискрет' }]}
				onChange={value => setForm({ ...form, TagType: value })}
				style={{ width: '100%' }}
			></Select>

			<div className="form-caption">Описание</div>
			<Input.TextArea
				value={form.Description}
				onChange={e => setForm({ ...form, Description: e.target.value })}
			/>

			<div className="form-caption">Используемый источник</div>
			<Select
				options={sources.map(x => ({ value: String(x.Id), label: x.Name }))}
				loading={isSourcesLoading}
				onChange={value => setForm({ ...form, SourceId: Number(value) })}
				style={{ width: '100%' }}
			></Select>

			<div className="form-caption">Путь к данным в источнике</div>
			<Input value={form.SourceItem} onChange={e => setForm({ ...form, SourceItem: e.target.value })} />

			<div className="form-caption">Интервал опроса в секундах (0, если только по изменению)</div>
			<InputNumber value={form.Interval} onChange={value => setForm({ ...form, Interval: Number(value) })} />
		</Modal>
	)
}